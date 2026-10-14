#nullable enable
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Security;
using AcxiomCRM.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly RecordAuthorizationService _authService;

    public ReportsController(IReportService reportService, RecordAuthorizationService authService)
    {
        _reportService = reportService; _authService = authService;
    }

    public async Task<IActionResult> CustomerReport(string? search, CustomerStatus? status, int page = 1, int pageSize = 10)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var report = await _reportService.GetCustomerReportAsync(search, status, ownerFilter, page, pageSize);
        ViewBag.Search = search; ViewBag.Status = status;
        return View(report);
    }

    public async Task<IActionResult> LeadReport(string? search, LeadStatus? status, int page = 1, int pageSize = 10)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var report = await _reportService.GetLeadReportAsync(search, status, ownerFilter, page, pageSize);
        ViewBag.Search = search; ViewBag.Status = status;
        return View(report);
    }

    public async Task<IActionResult> FollowUpReport(string? search, FollowUpStatus? status, int page = 1, int pageSize = 10)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var report = await _reportService.GetFollowUpReportAsync(search, status, ownerFilter, page, pageSize);
        return View(report);
    }

    public async Task<IActionResult> OpportunityReport(string? search, OpportunityStage? stage, int page = 1, int pageSize = 10)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var report = await _reportService.GetOpportunityReportAsync(search, stage, ownerFilter, page, pageSize);
        ViewBag.Search = search; ViewBag.Stage = stage;
        return View(report);
    }

    public async Task<IActionResult> PipelineReport()
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var report = await _reportService.GetPipelineReportAsync(ownerFilter);
        return View(report);
    }

    public async Task<IActionResult> ConversionReport()
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var report = await _reportService.GetConversionReportAsync(ownerFilter);
        return View(report);
    }

    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> UserActivityReport(string? userId, int page = 1, int pageSize = 10)
    {
        var report = await _reportService.GetUserActivityReportAsync(userId, page, pageSize);
        return View(report);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AuditReport(string? userId, string? action, string? entity, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        var report = await _reportService.GetAuditReportAsync(userId, action, entity, fromDate, toDate, page, pageSize);
        return View(report);
    }

    public async Task<IActionResult> ExportCustomersCsv(string? search, CustomerStatus? status)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var csv = await _reportService.ExportCustomerReportCsvAsync(search, status, ownerFilter);
        return File(csv, "text/csv", $"customers_{DateTime.Now:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> ExportLeadsCsv(string? search, LeadStatus? status)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var csv = await _reportService.ExportLeadReportCsvAsync(search, status, ownerFilter);
        return File(csv, "text/csv", $"leads_{DateTime.Now:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> ExportOpportunitiesCsv(string? search, OpportunityStage? stage)
    {
        var ownerFilter = _authService.GetOwnerFilter(User);
        var csv = await _reportService.ExportOpportunityReportCsvAsync(search, stage, ownerFilter);
        return File(csv, "text/csv", $"opportunities_{DateTime.Now:yyyyMMdd}.csv");
    }
}
