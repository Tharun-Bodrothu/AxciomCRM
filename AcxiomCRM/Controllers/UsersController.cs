#nullable enable
using AcxiomCRM.Models;
using AcxiomCRM.Security;
using AcxiomCRM.Services.Interfaces;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers;

[Authorize(Policy = "AdminOnly")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAuditService _auditService;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IAuditService auditService)
    {
        _userManager = userManager; _roleManager = roleManager; _auditService = auditService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        var users = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            users = users.Where(u => u.Email!.ToLower().Contains(s) || u.FirstName.ToLower().Contains(s) || u.LastName.ToLower().Contains(s));
        }

        var userList = users.OrderBy(u => u.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = users.Count();

        var viewModels = new List<UserViewModel>();
        foreach (var user in userList)
        {
            var roles = await _userManager.GetRolesAsync(user);
            viewModels.Add(new UserViewModel
            {
                Id = user.Id, Email = user.Email ?? "", FirstName = user.FirstName, LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "None", IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate, IsLockedOut = await _userManager.IsLockedOutAsync(user),
                CreatedDate = user.CreatedDate
            });
        }

        ViewBag.Search = search; ViewBag.Page = page; ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return View(viewModels);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserViewModel
        {
            Id = user.Id, Email = user.Email ?? "", FirstName = user.FirstName, LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "None", IsActive = user.IsActive,
            LastLoginDate = user.LastLoginDate, IsLockedOut = await _userManager.IsLockedOutAsync(user),
            CreatedDate = user.CreatedDate
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserViewModel
        {
            Id = user.Id, Email = user.Email ?? "", FirstName = user.FirstName, LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "None", IsActive = user.IsActive
        };
        ViewBag.Roles = new SelectList(AuthorizationPolicies.AllRoleNames, model.Role);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any()) await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);

        user.IsActive = model.IsActive;
        await _userManager.UpdateAsync(user);

        var adminId = _userManager.GetUserId(User) ?? "";
        await _auditService.LogAsync(adminId, User.Identity?.Name ?? "", "UserUpdated", "User", id,
            $"Role: {string.Join(",", currentRoles)}", $"Role: {model.Role}, Active: {model.IsActive}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        var adminId = _userManager.GetUserId(User) ?? "";
        await _auditService.LogAsync(adminId, User.Identity?.Name ?? "",
            user.IsActive ? "UserActivated" : "UserDeactivated", "User", id, null, $"Active: {user.IsActive}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            var adminId = _userManager.GetUserId(User) ?? "";
            await _auditService.LogAsync(adminId, User.Identity?.Name ?? "", "PasswordReset", "User", id,
                null, null, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["SuccessMessage"] = "Password reset successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockAccount(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);

        var adminId = _userManager.GetUserId(User) ?? "";
        await _auditService.LogAsync(adminId, User.Identity?.Name ?? "", "AccountUnlocked", "User", id,
            null, null, HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["SuccessMessage"] = "Account unlocked successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
