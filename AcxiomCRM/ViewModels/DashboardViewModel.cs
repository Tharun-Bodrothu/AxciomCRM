#nullable enable
namespace AcxiomCRM.ViewModels;

public class DashboardViewModel
{
    public int TotalCustomers { get; set; }
    public int TotalLeads { get; set; }
    public int OpenLeads { get; set; }
    public int TotalOpportunities { get; set; }
    public int OpenOpportunities { get; set; }
    public int WonOpportunities { get; set; }
    public int LostOpportunities { get; set; }
    public decimal TotalPipelineValue { get; set; }

    public List<ChartDataItem> LeadStatusData { get; set; } = new();
    public List<ChartDataItem> OpportunityPipelineData { get; set; } = new();
    public List<ChartDataItem> MonthlySalesData { get; set; } = new();
}

public class ChartDataItem
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Value { get; set; }
}
