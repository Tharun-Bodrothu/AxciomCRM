#nullable enable
using AcxiomCRM.Data;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services.Interfaces;
using AcxiomCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context) => _context = context;

    public async Task<DashboardViewModel> GetDashboardDataAsync(string userId, string role, DateTime? startDate, DateTime? endDate)
    {
        // Determine owner filter based on role
        string? ownerFilter = role == "SalesExecutive" ? userId : null;

        var model = new DashboardViewModel();

        // KPI counts
        var customerQuery = _context.Customers.AsQueryable();
        var leadQuery = _context.Leads.AsQueryable();
        var oppQuery = _context.Opportunities.AsQueryable();

        if (ownerFilter != null)
        {
            customerQuery = customerQuery.Where(c => c.OwnerId == ownerFilter);
            leadQuery = leadQuery.Where(l => l.AssignedTo == ownerFilter);
            oppQuery = oppQuery.Where(o => o.AssignedTo == ownerFilter);
        }

        if (startDate.HasValue) { leadQuery = leadQuery.Where(l => l.CreatedDate >= startDate.Value); oppQuery = oppQuery.Where(o => o.CreatedDate >= startDate.Value); }
        if (endDate.HasValue) { leadQuery = leadQuery.Where(l => l.CreatedDate <= endDate.Value); oppQuery = oppQuery.Where(o => o.CreatedDate <= endDate.Value); }

        model.TotalCustomers = await customerQuery.CountAsync();
        model.TotalLeads = await leadQuery.CountAsync();
        model.OpenLeads = await leadQuery.Where(l => l.Status != LeadStatus.Converted && l.Status != LeadStatus.Lost && l.Status != LeadStatus.Unqualified).CountAsync();
        model.TotalOpportunities = await oppQuery.CountAsync();
        model.OpenOpportunities = await oppQuery.Where(o => o.Stage != OpportunityStage.Won && o.Stage != OpportunityStage.Lost).CountAsync();
        model.WonOpportunities = await oppQuery.Where(o => o.Stage == OpportunityStage.Won).CountAsync();
        model.LostOpportunities = await oppQuery.Where(o => o.Stage == OpportunityStage.Lost).CountAsync();
        model.TotalPipelineValue = await oppQuery.Where(o => o.Stage != OpportunityStage.Won && o.Stage != OpportunityStage.Lost).SumAsync(o => o.Amount);

        // Lead Status Chart Data
        model.LeadStatusData = await leadQuery.GroupBy(l => l.Status)
            .Select(g => new ChartDataItem { Label = g.Key.ToString(), Count = g.Count() }).ToListAsync();

        // Opportunity Pipeline Chart Data
        model.OpportunityPipelineData = await oppQuery.GroupBy(o => o.Stage)
            .Select(g => new ChartDataItem { Label = g.Key.ToString(), Count = g.Count(), Value = g.Sum(o => o.Amount) }).ToListAsync();

        // Monthly Sales Data (last 6 months)
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var wonQuery = _context.Opportunities.Where(o => o.Stage == OpportunityStage.Won && o.ModifiedDate >= sixMonthsAgo);
        if (ownerFilter != null) wonQuery = wonQuery.Where(o => o.AssignedTo == ownerFilter);

        model.MonthlySalesData = await wonQuery
            .GroupBy(o => new { o.ModifiedDate!.Value.Year, o.ModifiedDate!.Value.Month })
            .Select(g => new ChartDataItem
            {
                Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                Count = g.Count(),
                Value = g.Sum(o => o.Amount)
            }).OrderBy(x => x.Label).ToListAsync();

        return model;
    }
}
