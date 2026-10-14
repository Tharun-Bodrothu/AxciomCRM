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
public class FollowUpsController : Controller
{
    private readonly IFollowUpService _followUpService;
    private readonly ICustomerService _customerService;
    private readonly ILeadService _leadService;
    private readonly IAuditService _auditService;
    private readonly RecordAuthorizationService _authService;
    private readonly UserManager<ApplicationUser> _userManager;

    public FollowUpsController(IFollowUpService followUpService, ICustomerService customerService,
        ILeadService leadService, IAuditService auditService, RecordAuthorizationService authService,
        UserManager<ApplicationUser> userManager)
    {
        _followUpService = followUpService; _customerService = customerService;
        _leadService = leadService; _auditService = auditService;
        _authService = authService; _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, FollowUpStatus? status, DateTime? fromDate, DateTime? toDate,
        int page = 1, int pageSize = 10, string? sortBy = null, bool sortDesc = false)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var followUps = await _followUpService.GetFollowUpsAsync(search, status, ownerFilter, fromDate, toDate, page, pageSize, sortBy, sortDesc);
        ViewBag.Search = search; ViewBag.Status = status; ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd"); ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(followUps);
    }

    public async Task<IActionResult> Upcoming()
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var followUps = await _followUpService.GetUpcomingAsync(ownerFilter);
        return View(followUps);
    }

    public async Task<IActionResult> Overdue()
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var followUps = await _followUpService.GetOverdueAsync(ownerFilter);
        return View(followUps);
    }

    public async Task<IActionResult> Details(int id)
    {
        var f = await _followUpService.GetByIdAsync(id);
        if (f == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, f.AssignedTo)) return Forbid();
        return View(f);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new FollowUpViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FollowUpViewModel model)
    {
        if (!ModelState.IsValid) { await PopulateDropdowns(); return View(model); }
        var errors = FollowUpValidator.Validate(model.FollowUpDate, model.Subject, model.Status);
        if (errors.Count > 0) { foreach (var e in errors) ModelState.AddModelError(string.Empty, e); await PopulateDropdowns(); return View(model); }

        var userId = _authService.GetUserId(User) ?? string.Empty;
        var followUp = new FollowUp
        {
            CustomerId = model.CustomerId, LeadId = model.LeadId, OpportunityId = model.OpportunityId,
            FollowUpDate = model.FollowUpDate, FollowUpType = model.FollowUpType,
            Subject = model.Subject, Remarks = model.Remarks, Status = FollowUpStatus.Planned,
            AssignedTo = model.AssignedTo ?? userId, CreatedDate = DateTime.UtcNow
        };

        var created = await _followUpService.CreateAsync(followUp);
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "FollowUpCreated", "FollowUp",
            created.FollowUpId.ToString(), null, $"Subject: {created.Subject}, Date: {created.FollowUpDate:yyyy-MM-dd}",
            HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["SuccessMessage"] = "Follow-up scheduled successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var f = await _followUpService.GetByIdAsync(id);
        if (f == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, f.AssignedTo)) return Forbid();

        var model = new FollowUpViewModel
        {
            FollowUpId = f.FollowUpId, CustomerId = f.CustomerId, LeadId = f.LeadId,
            OpportunityId = f.OpportunityId, FollowUpDate = f.FollowUpDate, FollowUpType = f.FollowUpType,
            Subject = f.Subject, Remarks = f.Remarks, Status = f.Status, AssignedTo = f.AssignedTo
        };
        await PopulateDropdowns(f.AssignedTo);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FollowUpViewModel model)
    {
        if (id != model.FollowUpId) return BadRequest();
        if (!ModelState.IsValid) { await PopulateDropdowns(model.AssignedTo); return View(model); }

        var existing = await _followUpService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, existing.AssignedTo)) return Forbid();

        existing.CustomerId = model.CustomerId; existing.LeadId = model.LeadId;
        existing.FollowUpDate = model.FollowUpDate; existing.FollowUpType = model.FollowUpType;
        existing.Subject = model.Subject; existing.Remarks = model.Remarks;
        existing.AssignedTo = model.AssignedTo ?? existing.AssignedTo; existing.ModifiedDate = DateTime.UtcNow;

        await _followUpService.UpdateAsync(existing);
        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "FollowUpUpdated", "FollowUp",
            id.ToString(), null, $"Subject: {existing.Subject}", HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["SuccessMessage"] = "Follow-up updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, string? remarks)
    {
        var f = await _followUpService.GetByIdAsync(id);
        if (f == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, f.AssignedTo)) return Forbid();
        await _followUpService.CompleteAsync(id, remarks);
        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "FollowUpCompleted", "FollowUp",
            id.ToString(), "Status: Planned", "Status: Completed", HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["SuccessMessage"] = "Follow-up marked as completed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(int id, DateTime newDate, string? remarks)
    {
        var f = await _followUpService.GetByIdAsync(id);
        if (f == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, f.AssignedTo)) return Forbid();
        try
        {
            await _followUpService.RescheduleAsync(id, newDate, remarks);
            var userId = _authService.GetUserId(User) ?? string.Empty;
            await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "FollowUpRescheduled", "FollowUp",
                id.ToString(), $"Date: {f.FollowUpDate:yyyy-MM-dd}", $"Date: {newDate:yyyy-MM-dd}", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["SuccessMessage"] = "Follow-up rescheduled successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? remarks)
    {
        var f = await _followUpService.GetByIdAsync(id);
        if (f == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, f.AssignedTo)) return Forbid();
        await _followUpService.CancelAsync(id, remarks);
        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "FollowUpCancelled", "FollowUp",
            id.ToString(), "Status: Planned", "Status: Cancelled", HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["SuccessMessage"] = "Follow-up cancelled.";
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
