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

/// <summary>
/// Manages Lead CRUD, status transitions, qualification, and conversion.
/// </summary>
[Authorize]
public class LeadsController : Controller
{
    private readonly ILeadService _leadService;
    private readonly IAuditService _auditService;
    private readonly RecordAuthorizationService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(
        ILeadService leadService,
        IAuditService auditService,
        RecordAuthorizationService authService,
        UserManager<ApplicationUser> userManager,
        ILogger<LeadsController> logger)
    {
        _leadService = leadService;
        _auditService = auditService;
        _authService = authService;
        _userManager = userManager;
        _logger = logger;
    }

    // ── List ───────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search, LeadStatus? status, LeadPriority? priority,
        int page = 1, int pageSize = 10, string? sortBy = null, bool sortDesc = false)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var leads = await _leadService.GetLeadsAsync(search, status, priority, ownerFilter, page, pageSize, sortBy, sortDesc);

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Priority = priority;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDesc = sortDesc;

        return View(leads);
    }

    // ── Details ────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var lead = await _leadService.GetByIdAsync(id);
        if (lead == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, lead.AssignedTo))
            return Forbid();

        return View(lead);
    }

    // ── Create ─────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new LeadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var errors = LeadValidator.Validate(model.LeadName, model.Email, model.ExpectedValue);
        if (errors.Count > 0)
        {
            foreach (var error in errors)
                ModelState.AddModelError(string.Empty, error);
            await PopulateDropdowns();
            return View(model);
        }

        var userId = _authService.GetUserId(User) ?? string.Empty;

        var lead = new Lead
        {
            LeadCode = await _leadService.GenerateLeadCodeAsync(),
            LeadName = model.LeadName,
            Email = model.Email,
            Phone = model.Phone,
            CompanyName = model.CompanyName,
            Source = model.Source,
            Status = LeadStatus.New,
            Priority = model.Priority,
            ExpectedValue = model.ExpectedValue,
            Notes = model.Notes,
            AssignedTo = model.AssignedTo ?? userId,
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        var created = await _leadService.CreateAsync(lead);

        await _auditService.LogAsync(userId, User.Identity?.Name ?? "unknown",
            "LeadCreated", "Lead", created.LeadId.ToString(),
            null, $"Name: {created.LeadName}, Email: {created.Email}, Status: New",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = "Lead created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var lead = await _leadService.GetByIdAsync(id);
        if (lead == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, lead.AssignedTo))
            return Forbid();

        var model = new LeadViewModel
        {
            LeadId = lead.LeadId,
            LeadCode = lead.LeadCode,
            LeadName = lead.LeadName,
            Email = lead.Email,
            Phone = lead.Phone,
            CompanyName = lead.CompanyName,
            Source = lead.Source,
            Status = lead.Status,
            Priority = lead.Priority,
            ExpectedValue = lead.ExpectedValue,
            Notes = lead.Notes,
            AssignedTo = lead.AssignedTo
        };

        await PopulateDropdowns(lead.AssignedTo);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LeadViewModel model)
    {
        if (id != model.LeadId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(model.AssignedTo);
            return View(model);
        }

        var existing = await _leadService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, existing.AssignedTo))
            return Forbid();

        var oldValue = $"Name: {existing.LeadName}, Status: {existing.Status}, Priority: {existing.Priority}";

        existing.LeadName = model.LeadName;
        existing.Email = model.Email;
        existing.Phone = model.Phone;
        existing.CompanyName = model.CompanyName;
        existing.Source = model.Source;
        existing.Priority = model.Priority;
        existing.ExpectedValue = model.ExpectedValue;
        existing.Notes = model.Notes;
        existing.AssignedTo = model.AssignedTo ?? existing.AssignedTo;
        existing.ModifiedDate = DateTime.UtcNow;

        await _leadService.UpdateAsync(existing);

        var userId = _authService.GetUserId(User) ?? string.Empty;
        var newValue = $"Name: {existing.LeadName}, Status: {existing.Status}, Priority: {existing.Priority}";
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "unknown",
            "LeadUpdated", "Lead", existing.LeadId.ToString(),
            oldValue, newValue, HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = "Lead updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Change Status ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, LeadStatus newStatus)
    {
        var lead = await _leadService.GetByIdAsync(id);
        if (lead == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, lead.AssignedTo))
            return Forbid();

        if (!await _leadService.IsValidStatusTransition(lead.Status, newStatus))
        {
            TempData["ErrorMessage"] = $"Cannot transition from {lead.Status} to {newStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var oldStatus = lead.Status;
        await _leadService.ChangeStatusAsync(id, newStatus);

        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "unknown",
            "LeadStatusChanged", "Lead", id.ToString(),
            $"Status: {oldStatus}", $"Status: {newStatus}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = $"Lead status changed to {newStatus}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Convert Lead ───────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Convert(int id)
    {
        var lead = await _leadService.GetByIdAsync(id);
        if (lead == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, lead.AssignedTo))
            return Forbid();

        if (lead.Status != LeadStatus.Qualified)
        {
            TempData["ErrorMessage"] = "Only qualified leads can be converted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new LeadConvertViewModel
        {
            LeadId = lead.LeadId,
            LeadName = lead.LeadName,
            Email = lead.Email,
            CompanyName = lead.CompanyName,
            OpportunityName = $"Opportunity from {lead.LeadName}",
            Amount = lead.ExpectedValue ?? 0,
            AssignedTo = lead.AssignedTo
        };

        await PopulateDropdowns(lead.AssignedTo);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Convert(int id, LeadConvertViewModel model)
    {
        if (id != model.LeadId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(model.AssignedTo);
            return View(model);
        }

        var lead = await _leadService.GetByIdAsync(id);
        if (lead == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, lead.AssignedTo))
            return Forbid();

        try
        {
            var (customer, opportunity) = await _leadService.ConvertLeadAsync(
                id, model.OpportunityName, model.Amount, model.AssignedTo ?? lead.AssignedTo);

            var userId = _authService.GetUserId(User) ?? string.Empty;
            await _auditService.LogAsync(userId, User.Identity?.Name ?? "unknown",
                "LeadConverted", "Lead", id.ToString(),
                $"Status: Qualified",
                $"Status: Converted, CustomerId: {customer.CustomerId}, OpportunityId: {opportunity.OpportunityId}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["SuccessMessage"] = $"Lead converted successfully. Customer: {customer.CustomerName}, Opportunity: {opportunity.OpportunityName}";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdowns(model.AssignedTo);
            return View(model);
        }
    }

    // ── Delete ─────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Delete(int id)
    {
        var lead = await _leadService.GetByIdAsync(id);
        if (lead == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, lead.AssignedTo))
            return Forbid();

        await _leadService.DeleteAsync(id);

        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "unknown",
            "LeadDeleted", "Lead", id.ToString(),
            $"Name: {lead.LeadName}", null,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = "Lead deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(string? selectedUserId = null)
    {
        var users = _userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FirstName).ToList();
        ViewBag.Users = new SelectList(users, "Id", "FullName", selectedUserId);
    }
}
