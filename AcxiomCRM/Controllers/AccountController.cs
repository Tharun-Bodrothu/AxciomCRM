#nullable enable
using AcxiomCRM.Models;
using AcxiomCRM.Security;
using AcxiomCRM.Services.Interfaces;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

/// <summary>
/// Handles user authentication: login, registration, logout, and access denied.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAuditService _auditService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IAuditService auditService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _auditService = auditService;
        _logger = logger;
    }

    // ── Login ──────────────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            await _auditService.LogLoginAsync("unknown", model.Email, false,
                HttpContext.Connection.RemoteIpAddress?.ToString(), "User not found");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (!user.IsActive)
        {
            await _auditService.LogLoginAsync(user.Id, user.Email ?? model.Email, false,
                HttpContext.Connection.RemoteIpAddress?.ToString(), "Account deactivated");
            ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact an administrator.");
            return View(model);
        }

        // Check if locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            await _auditService.LogLoginAsync(user.Id, user.Email ?? model.Email, false,
                HttpContext.Connection.RemoteIpAddress?.ToString(), "Account locked out");
            ModelState.AddModelError(string.Empty, "Your account is temporarily locked due to multiple failed login attempts. Please try again later.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _auditService.LogLoginAsync(user.Id, user.Email ?? model.Email, true,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            _logger.LogInformation("User {Email} logged in successfully.", model.Email);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            await _auditService.LogLoginAsync(user.Id, user.Email ?? model.Email, false,
                HttpContext.Connection.RemoteIpAddress?.ToString(), "Account locked out after failed attempts");
            _logger.LogWarning("User {Email} account locked out.", model.Email);
            ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed login attempts. Please try again in 15 minutes.");
            return View(model);
        }

        await _auditService.LogLoginAsync(user.Id, user.Email ?? model.Email, false,
            HttpContext.Connection.RemoteIpAddress?.ToString(), "Invalid password");
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    // ── Register ───────────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Validate role
        if (!AuthorizationPolicies.AllRoleNames.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Invalid role selected.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);

            var adminId = _userManager.GetUserId(User);
            await _auditService.LogAsync(adminId ?? "system", User.Identity?.Name ?? "system",
                "UserCreated", "User", user.Id, null, $"Email: {user.Email}, Role: {model.Role}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            _logger.LogInformation("Admin created new user {Email} with role {Role}.", model.Email, model.Role);
            TempData["SuccessMessage"] = $"User {model.Email} created successfully with role {model.Role}.";
            return RedirectToAction("Index", "Users");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // ── Logout ─────────────────────────────────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        var email = User.Identity?.Name ?? "unknown";

        await _signInManager.SignOutAsync();

        await _auditService.LogAsync(userId ?? "unknown", email,
            "Logout", "User", userId, null, null,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        _logger.LogInformation("User {Email} logged out.", email);
        return RedirectToAction("Login");
    }

    // ── Access Denied ──────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
