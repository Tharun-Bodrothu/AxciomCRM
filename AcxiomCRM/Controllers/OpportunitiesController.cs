#nullable enable
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Security;
using AcxiomCRM.Services.Interfaces;
using AcxiomCRM.Validators;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers;

[Authorize]
public class OpportunitiesController : Controller
{
    private readonly IOpportunityService _opportunityService;
    private readonly ICustomerService _customerService;
    private readonly IAuditService _auditService;
    private readonly RecordAuthorizationService _authService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OpportunitiesController(IOpportunityService opportunityService, ICustomerService customerService,
        IAuditService auditService, RecordAuthorizationService authService, UserManager<ApplicationUser> userManager)
    {
        _opportunityService = opportunityService;
        _customerService = customerService;
        _auditService = auditService;
        _authService = authService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, OpportunityStage? stage,
        int page = 1, int pageSize = 10, string? sortBy = null, bool sortDesc = false)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var opps = await _opportunityService.GetOpportunitiesAsync(search, stage, ownerFilter, page, pageSize, sortBy, sortDesc);
        ViewBag.Search = search; ViewBag.Stage = stage; ViewBag.SortBy = sortBy; ViewBag.SortDesc = sortDesc;
        return View(opps);
    }

    public async Task<IActionResult> Details(int id)
    {
        var opp = await _opportunityService.GetByIdAsync(id);
        if (opp == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, opp.AssignedTo)) return Forbid();
        return View(opp);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new OpportunityViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OpportunityViewModel model)
    {
        if (!ModelState.IsValid) { await PopulateDropdowns(); return View(model); }

        var errors = OpportunityValidator.Validate(model.Amount, model.Probability, model.ExpectedCloseDate, model.Stage);
        if (errors.Count > 0) { foreach (var e in errors) ModelState.AddModelError(string.Empty, e); await PopulateDropdowns(); return View(model); }

        var userId = _authService.GetUserId(User) ?? string.Empty;
        var opp = new Opportunity
        {
            OpportunityName = model.OpportunityName, CustomerId = model.CustomerId, LeadId = model.LeadId,
            Amount = model.Amount, Stage = model.Stage, Probability = model.Probability,
            ExpectedCloseDate = model.ExpectedCloseDate, Notes = model.Notes,
            AssignedTo = model.AssignedTo ?? userId, CreatedBy = userId, CreatedDate = DateTime.UtcNow
        };

        var created = await _opportunityService.CreateAsync(opp);
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "OpportunityCreated", "Opportunity",
            created.OpportunityId.ToString(), null, $"Name: {created.OpportunityName}, Amount: {created.Amount}, Stage: {created.Stage}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = "Opportunity created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var opp = await _opportunityService.GetByIdAsync(id);
        if (opp == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, opp.AssignedTo)) return Forbid();

        var model = new OpportunityViewModel
        {
            OpportunityId = opp.OpportunityId, OpportunityName = opp.OpportunityName,
            CustomerId = opp.CustomerId, LeadId = opp.LeadId, Amount = opp.Amount,
            Stage = opp.Stage, Probability = opp.Probability, ExpectedCloseDate = opp.ExpectedCloseDate,
            Notes = opp.Notes, AssignedTo = opp.AssignedTo
        };
        await PopulateDropdowns(opp.AssignedTo);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OpportunityViewModel model)
    {
        if (id != model.OpportunityId) return BadRequest();
        if (!ModelState.IsValid) { await PopulateDropdowns(model.AssignedTo); return View(model); }

        var existing = await _opportunityService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, existing.AssignedTo)) return Forbid();

        var errors = OpportunityValidator.Validate(model.Amount, model.Probability, model.ExpectedCloseDate, model.Stage);
        if (errors.Count > 0) { foreach (var e in errors) ModelState.AddModelError(string.Empty, e); await PopulateDropdowns(model.AssignedTo); return View(model); }

        var oldValue = $"Name: {existing.OpportunityName}, Amount: {existing.Amount}, Stage: {existing.Stage}";
        existing.OpportunityName = model.OpportunityName; existing.CustomerId = model.CustomerId;
        existing.Amount = model.Amount; existing.Stage = model.Stage; existing.Probability = model.Probability;
        existing.ExpectedCloseDate = model.ExpectedCloseDate; existing.Notes = model.Notes;
        existing.AssignedTo = model.AssignedTo ?? existing.AssignedTo; existing.ModifiedDate = DateTime.UtcNow;

        await _opportunityService.UpdateAsync(existing);
        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "OpportunityUpdated", "Opportunity",
            existing.OpportunityId.ToString(), oldValue, $"Name: {existing.OpportunityName}, Amount: {existing.Amount}, Stage: {existing.Stage}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = "Opportunity updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStage(int id, OpportunityStage newStage)
    {
        var opp = await _opportunityService.GetByIdAsync(id);
        if (opp == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, opp.AssignedTo)) return Forbid();

        var oldStage = opp.Stage;
        await _opportunityService.ChangeStageAsync(id, newStage);
        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "OpportunityStageChanged", "Opportunity",
            id.ToString(), $"Stage: {oldStage}", $"Stage: {newStage}", HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = $"Opportunity stage changed to {newStage}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Delete(int id)
    {
        var opp = await _opportunityService.GetByIdAsync(id);
        if (opp == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, opp.AssignedTo)) return Forbid();
        await _opportunityService.DeleteAsync(id);
        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "OpportunityDeleted", "Opportunity",
            id.ToString(), $"Name: {opp.OpportunityName}", null, HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["SuccessMessage"] = "Opportunity deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(string? selectedUserId = null)
    {
        var users = _userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FirstName).ToList();
        ViewBag.Users = new SelectList(users, "Id", "FullName", selectedUserId);
        var ownerFilter = _authService.GetOwnerFilter(User);
        var customers = await _customerService.GetCustomersAsync(null, null, ownerFilter, 1, 1000, null, false);
        ViewBag.Customers = new SelectList(customers, "CustomerId", "CustomerName");
    }
}
