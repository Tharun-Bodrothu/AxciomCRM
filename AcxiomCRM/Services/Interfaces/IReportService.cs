#nullable enable
using AcxiomCRM.Models.Enums;
using AcxiomCRM.ViewModels;

namespace AcxiomCRM.Services.Interfaces;

public interface IReportService
{
    Task<ReportDataViewModel<CustomerReportItem>> GetCustomerReportAsync(string? search, CustomerStatus? status, string? ownerId, int page, int pageSize);
    Task<ReportDataViewModel<LeadReportItem>> GetLeadReportAsync(string? search, LeadStatus? status, string? assignedTo, int page, int pageSize);
    Task<ReportDataViewModel<FollowUpReportItem>> GetFollowUpReportAsync(string? search, FollowUpStatus? status, string? assignedTo, int page, int pageSize);
    Task<ReportDataViewModel<OpportunityReportItem>> GetOpportunityReportAsync(string? search, OpportunityStage? stage, string? assignedTo, int page, int pageSize);
    Task<PipelineReportViewModel> GetPipelineReportAsync(string? assignedTo);
    Task<ConversionReportViewModel> GetConversionReportAsync(string? assignedTo);
    Task<ReportDataViewModel<UserActivityReportItem>> GetUserActivityReportAsync(string? userId, int page, int pageSize);
    Task<ReportDataViewModel<AuditReportItem>> GetAuditReportAsync(string? userId, string? action, string? entity, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<byte[]> ExportCustomerReportCsvAsync(string? search, CustomerStatus? status, string? ownerId);
    Task<byte[]> ExportLeadReportCsvAsync(string? search, LeadStatus? status, string? assignedTo);
    Task<byte[]> ExportOpportunityReportCsvAsync(string? search, OpportunityStage? stage, string? assignedTo);
}
