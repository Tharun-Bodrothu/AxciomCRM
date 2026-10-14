#nullable enable
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Security;
using AcxiomCRM.Services.Interfaces;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers;

[Authorize]
public class ActivitiesController : Controller
{
    private readonly IActivityService _activityService;
    private readonly ICustomerService _customerService;
    private readonly IAuditService _auditService;
    private readonly RecordAuthorizationService _authService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ActivitiesController(IActivityService activityService, ICustomerService customerService,
        IAuditService auditService, RecordAuthorizationService authService, UserManager<ApplicationUser> userManager)
    {
        _activityService = activityService; _customerService = customerService;
        _auditService = auditService; _authService = authService; _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, ActivityType? type, DateTime? fromDate, DateTime? toDate,
        int page = 1, int pageSize = 10, string? sortBy = null, bool sortDesc = false)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var activities = await _activityService.GetActivitiesAsync(search, type, ownerFilter, fromDate, toDate, page, pageSize, sortBy, sortDesc);
        ViewBag.Search = search; ViewBag.Type = type;
        return View(activities);
    }

    public async Task<IActionResult> Details(int id)
    {
        var a = await _activityService.GetByIdAsync(id);
        if (a == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, a.AssignedTo)) return Forbid();
        return View(a);
    }

    [HttpGet]
    public async Task<IActionResult> Create() { await PopulateDropdowns(); return View(new ActivityViewModel()); }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActivityViewModel model)
    {
        if (!ModelState.IsValid) { await PopulateDropdowns(); return View(model); }
        var userId = _authService.GetUserId(User) ?? string.Empty;
        var activity = new Activity
        {
            Type = model.Type, Subject = model.Subject, Description = model.Description,
            ActivityDate = model.ActivityDate, CustomerId = model.CustomerId, LeadId = model.LeadId,
            AssignedTo = model.AssignedTo ?? userId, IsCompleted = model.IsCompleted,
            CreatedBy = userId, CreatedDate = DateTime.UtcNow
        };
        var created = await _activityService.CreateAsync(activity);
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "ActivityCreated", "Activity",
            created.ActivityId.ToString(), null, $"Type: {created.Type}, Subject: {created.Subject}",
            HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["SuccessMessage"] = "Activity created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var a = await _activityService.GetByIdAsync(id);
        if (a == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, a.AssignedTo)) return Forbid();
        var model = new ActivityViewModel
        {
            ActivityId = a.ActivityId, Type = a.Type, Subject = a.Subject, Description = a.Description,
            ActivityDate = a.ActivityDate, CustomerId = a.CustomerId, LeadId = a.LeadId,
            AssignedTo = a.AssignedTo, IsCompleted = a.IsCompleted
        };
        await PopulateDropdowns(a.AssignedTo);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActivityViewModel model)
    {
        if (id != model.ActivityId) return BadRequest();
        if (!ModelState.IsValid) { await PopulateDropdowns(model.AssignedTo); return View(model); }
        var existing = await _activityService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, existing.AssignedTo)) return Forbid();

        existing.Type = model.Type; existing.Subject = model.Subject; existing.Description = model.Description;
        existing.ActivityDate = model.ActivityDate; existing.CustomerId = model.CustomerId; existing.LeadId = model.LeadId;
        existing.AssignedTo = model.AssignedTo ?? existing.AssignedTo; existing.IsCompleted = model.IsCompleted;
        existing.ModifiedDate = DateTime.UtcNow;

        await _activityService.UpdateAsync(existing);
        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "", "ActivityUpdated", "Activity",
            id.ToString(), null, $"Type: {existing.Type}, Subject: {existing.Subject}",
            HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["SuccessMessage"] = "Activity updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _activityService.GetByIdAsync(id);
        if (a == null) return NotFound();
        if (!await _authService.CanAccessRecordAsync(User, a.AssignedTo)) return Forbid();
        await _activityService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Activity deleted successfully.";
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
