#nullable enable
using AcxiomCRM.Security;
using AcxiomCRM.Services.Interfaces;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

/// <summary>
/// Dashboard controller showing KPI cards and Chart.js charts.
/// Data is filtered by role and ownership.
/// </summary>
[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly RecordAuthorizationService _authService;

    public DashboardController(
        IDashboardService dashboardService,
        RecordAuthorizationService authService)
    {
        _dashboardService = dashboardService;
        _authService = authService;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var userId = _authService.GetUserId(User) ?? string.Empty;
        var role = _authService.GetUserRole(User);

        var model = await _dashboardService.GetDashboardDataAsync(userId, role, startDate, endDate);

        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

        return View(model);
    }
}
