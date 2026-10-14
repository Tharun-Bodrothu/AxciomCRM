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
/// Manages Customer CRUD operations with search, filtering, pagination,
/// and record-level authorization.
/// </summary>
[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IAuditService _auditService;
    private readonly RecordAuthorizationService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        IAuditService auditService,
        RecordAuthorizationService authService,
        UserManager<ApplicationUser> userManager,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _auditService = auditService;
        _authService = authService;
        _userManager = userManager;
        _logger = logger;
    }

    // ── List ───────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search, CustomerStatus? status,
        int page = 1, int pageSize = 10, string? sortBy = null, bool sortDesc = false)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var customers = await _customerService.GetCustomersAsync(search, status, ownerFilter, page, pageSize, sortBy, sortDesc);

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDesc = sortDesc;

        return View(customers);
    }

    // ── Details ────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, customer.OwnerId))
            return Forbid();

        return View(customer);
    }

    // ── Create ─────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateOwnerDropdown();
        return View(new CustomerViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOwnerDropdown();
            return View(model);
        }

        // Business validation
        var errors = CustomerValidator.Validate(model.CustomerName, model.Email, model.Phone, model.CompanyName);
        if (errors.Count > 0)
        {
            foreach (var error in errors)
                ModelState.AddModelError(string.Empty, error);
            await PopulateOwnerDropdown();
            return View(model);
        }

        // Uniqueness checks
        if (!await _customerService.IsEmailUniqueAsync(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "A customer with this email already exists.");
            await PopulateOwnerDropdown();
            return View(model);
        }

        if (!await _customerService.IsPhoneUniqueAsync(model.Phone))
        {
            ModelState.AddModelError(nameof(model.Phone), "A customer with this phone number already exists.");
            await PopulateOwnerDropdown();
            return View(model);
        }

        var userId = _authService.GetUserId(User) ?? string.Empty;

        var customer = new Customer
        {
            CustomerCode = await _customerService.GenerateCustomerCodeAsync(),
            CustomerName = model.CustomerName,
            Email = model.Email,
            Phone = model.Phone,
            CompanyName = model.CompanyName,
            Address = model.Address,
            City = model.City,
            State = model.State,
            Status = model.Status,
            OwnerId = model.OwnerId ?? userId,
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            var created = await _customerService.CreateAsync(customer);
            await _auditService.LogAsync(userId, User.Identity?.Name ?? "unknown",
                "CustomerCreated", "Customer", created.CustomerId.ToString(),
                null, $"Name: {created.CustomerName}, Email: {created.Email}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["SuccessMessage"] = "Customer created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateOwnerDropdown();
            return View(model);
        }
    }

    // ── Edit ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, customer.OwnerId))
            return Forbid();

        var model = new CustomerViewModel
        {
            CustomerId = customer.CustomerId,
            CustomerCode = customer.CustomerCode,
            CustomerName = customer.CustomerName,
            Email = customer.Email,
            Phone = customer.Phone,
            CompanyName = customer.CompanyName,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            Status = customer.Status,
            OwnerId = customer.OwnerId
        };

        await PopulateOwnerDropdown(customer.OwnerId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerViewModel model)
    {
        if (id != model.CustomerId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateOwnerDropdown(model.OwnerId);
            return View(model);
        }

        var existing = await _customerService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, existing.OwnerId))
            return Forbid();

        // Business validation
        var errors = CustomerValidator.Validate(model.CustomerName, model.Email, model.Phone, model.CompanyName);
        if (errors.Count > 0)
        {
            foreach (var error in errors)
                ModelState.AddModelError(string.Empty, error);
            await PopulateOwnerDropdown(model.OwnerId);
            return View(model);
        }

        // Uniqueness checks (excluding current record)
        if (!await _customerService.IsEmailUniqueAsync(model.Email, id))
        {
            ModelState.AddModelError(nameof(model.Email), "A customer with this email already exists.");
            await PopulateOwnerDropdown(model.OwnerId);
            return View(model);
        }

        if (!await _customerService.IsPhoneUniqueAsync(model.Phone, id))
        {
            ModelState.AddModelError(nameof(model.Phone), "A customer with this phone number already exists.");
            await PopulateOwnerDropdown(model.OwnerId);
            return View(model);
        }

        var oldValue = $"Name: {existing.CustomerName}, Email: {existing.Email}, Phone: {existing.Phone}, Status: {existing.Status}";

        existing.CustomerName = model.CustomerName;
        existing.Email = model.Email;
        existing.Phone = model.Phone;
        existing.CompanyName = model.CompanyName;
        existing.Address = model.Address;
        existing.City = model.City;
        existing.State = model.State;
        existing.Status = model.Status;
        existing.OwnerId = model.OwnerId ?? existing.OwnerId;
        existing.ModifiedDate = DateTime.UtcNow;

        try
        {
            await _customerService.UpdateAsync(existing);

            var userId = _authService.GetUserId(User) ?? string.Empty;
            var newValue = $"Name: {existing.CustomerName}, Email: {existing.Email}, Phone: {existing.Phone}, Status: {existing.Status}";
            await _auditService.LogAsync(userId, User.Identity?.Name ?? "unknown",
                "CustomerUpdated", "Customer", existing.CustomerId.ToString(),
                oldValue, newValue, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["SuccessMessage"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateOwnerDropdown(model.OwnerId);
            return View(model);
        }
    }

    // ── Delete ─────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null) return NotFound();

        if (!await _authService.CanAccessRecordAsync(User, customer.OwnerId))
            return Forbid();

        // Soft-delete: change status to Inactive
        var oldValue = $"Status: {customer.Status}";
        customer.Status = CustomerStatus.Inactive;
        customer.ModifiedDate = DateTime.UtcNow;
        await _customerService.UpdateAsync(customer);

        var userId = _authService.GetUserId(User) ?? string.Empty;
        await _auditService.LogAsync(userId, User.Identity?.Name ?? "unknown",
            "CustomerDeactivated", "Customer", customer.CustomerId.ToString(),
            oldValue, "Status: Inactive", HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = "Customer deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ────────────────────────────────────────────────────────
    private async Task PopulateOwnerDropdown(string? selectedId = null)
    {
        var users = _userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FirstName).ToList();
        ViewBag.Owners = new SelectList(users, "Id", "FullName", selectedId);
    }
}
