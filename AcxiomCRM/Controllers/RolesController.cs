#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

[Authorize(Policy = "AdminOnly")]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager) => _roleManager = roleManager;

    public IActionResult Index()
    {
        var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        return View(roles);
    }
}
