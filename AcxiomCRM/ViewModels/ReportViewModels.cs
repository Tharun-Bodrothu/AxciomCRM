#nullable enable
using AcxiomCRM.Services;

namespace AcxiomCRM.ViewModels;

// ── Generic report data wrapper ────────────────────────────────────────
public class ReportDataViewModel<T>
{
    public PaginatedList<T> Items { get; set; } = new();
    public string? Search { get; set; }
    public string? FilterStatus { get; set; }
    public string? FilterOwner { get; set; }
}

// ── Customer Report ────────────────────────────────────────────────────
public class CustomerReportItem
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

// ── Lead Report ────────────────────────────────────────────────────────
public class LeadReportItem
{
    public string LeadCode { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string AssignedToName { get; set; } = string.Empty;
    public decimal? ExpectedValue { get; set; }
    public bool IsConverted { get; set; }
    public DateTime CreatedDate { get; set; }
}

// ── FollowUp Report ────────────────────────────────────────────────────
public class FollowUpReportItem
{
    public string Subject { get; set; } = string.Empty;
    public string FollowUpType { get; set; } = string.Empty;
    public DateTime FollowUpDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AssignedToName { get; set; } = string.Empty;
    public string? RelatedEntity { get; set; }
    public bool IsOverdue { get; set; }
}

// ── Opportunity Report ─────────────────────────────────────────────────
public class OpportunityReportItem
{
    public string OpportunityName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string Stage { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Probability { get; set; }
    public decimal WeightedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
}

// ── Pipeline Report ────────────────────────────────────────────────────
public class PipelineReportViewModel
{
    public List<PipelineStageItem> ByStage { get; set; } = new();
    public List<PipelineOwnerItem> ByOwner { get; set; } = new();
    public decimal TotalPipeline { get; set; }
    public decimal WeightedPipeline { get; set; }
}

public class PipelineStageItem
{
    public string Stage { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal WeightedAmount { get; set; }
}

public class PipelineOwnerItem
{
    public string OwnerName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal WeightedAmount { get; set; }
}

// ── Conversion Report ──────────────────────────────────────────────────
public class ConversionReportViewModel
{
    public int TotalLeads { get; set; }
    public int ConvertedLeads { get; set; }
    public int NotConvertedLeads { get; set; }
    public decimal ConversionRate { get; set; }
    public List<ChartDataItem> ConversionData { get; set; } = new();
}

// ── User Activity Report ───────────────────────────────────────────────
public class UserActivityReportItem
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int CustomersOwned { get; set; }
    public int LeadsAssigned { get; set; }
    public int OpportunitiesAssigned { get; set; }
    public int ActivitiesCompleted { get; set; }
    public int FollowUpsCompleted { get; set; }
}

// ── Audit Report ───────────────────────────────────────────────────────
public class AuditReportItem
{
    public long AuditLogId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string? Result { get; set; }
    public DateTime CreatedDate { get; set; }
}
