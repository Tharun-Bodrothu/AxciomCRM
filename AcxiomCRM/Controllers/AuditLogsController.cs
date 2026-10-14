#nullable enable
using AcxiomCRM.Services.Interfaces;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AuditLogsController : Controller
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService) => _auditService = auditService;

    public async Task<IActionResult> Index(string? userId, string? action, string? entity,
        DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20)
    {
        var logs = await _auditService.GetAuditLogsAsync(userId, action, entity, fromDate, toDate, page, pageSize);
        ViewBag.UserId = userId; ViewBag.Action = action; ViewBag.Entity = entity;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd"); ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(logs);
    }

    public async Task<IActionResult> Details(long id)
    {
        var logs = await _auditService.GetAuditLogsAsync(null, null, null, null, null, 1, int.MaxValue);
        var log = logs.FirstOrDefault(l => l.AuditLogId == id);
        if (log == null) return NotFound();
        return View(log);
    }
}
