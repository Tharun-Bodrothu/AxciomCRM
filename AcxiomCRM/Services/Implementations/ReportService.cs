#nullable enable
using System.Text;
using AcxiomCRM.Data;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services.Interfaces;
using AcxiomCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services.Implementations;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context) => _context = context;

    public async Task<ReportDataViewModel<CustomerReportItem>> GetCustomerReportAsync(string? search, CustomerStatus? status, string? ownerId, int page, int pageSize)
    {
        var query = _context.Customers.Include(c => c.Owner).AsQueryable();
        if (!string.IsNullOrWhiteSpace(ownerId)) query = query.Where(c => c.OwnerId == ownerId);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search)) { var s = search.ToLower(); query = query.Where(c => c.CustomerName.ToLower().Contains(s) || c.Email.ToLower().Contains(s)); }

        var items = query.Select(c => new CustomerReportItem
        {
            CustomerCode = c.CustomerCode, CustomerName = c.CustomerName, Email = c.Email,
            CompanyName = c.CompanyName, Status = c.Status.ToString(),
            OwnerName = c.Owner.FirstName + " " + c.Owner.LastName, CreatedDate = c.CreatedDate
        }).OrderByDescending(c => c.CreatedDate);

        return new ReportDataViewModel<CustomerReportItem>
        {
            Items = await PaginatedList<CustomerReportItem>.CreateAsync(items, page, pageSize),
            Search = search, FilterStatus = status?.ToString(), FilterOwner = ownerId
        };
    }

    public async Task<ReportDataViewModel<LeadReportItem>> GetLeadReportAsync(string? search, LeadStatus? status, string? assignedTo, int page, int pageSize)
    {
        var query = _context.Leads.Include(l => l.AssignedUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(l => l.AssignedTo == assignedTo);
        if (status.HasValue) query = query.Where(l => l.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search)) { var s = search.ToLower(); query = query.Where(l => l.LeadName.ToLower().Contains(s)); }

        var items = query.Select(l => new LeadReportItem
        {
            LeadCode = l.LeadCode, LeadName = l.LeadName, Source = l.Source, Status = l.Status.ToString(),
            Priority = l.Priority.ToString(), AssignedToName = l.AssignedUser.FirstName + " " + l.AssignedUser.LastName,
            ExpectedValue = l.ExpectedValue, IsConverted = l.Status == LeadStatus.Converted, CreatedDate = l.CreatedDate
        }).OrderByDescending(l => l.CreatedDate);

        return new ReportDataViewModel<LeadReportItem> { Items = await PaginatedList<LeadReportItem>.CreateAsync(items, page, pageSize), Search = search };
    }

    public async Task<ReportDataViewModel<FollowUpReportItem>> GetFollowUpReportAsync(string? search, FollowUpStatus? status, string? assignedTo, int page, int pageSize)
    {
        var query = _context.FollowUps.Include(f => f.AssignedUser).Include(f => f.Customer).Include(f => f.Lead).AsQueryable();
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(f => f.AssignedTo == assignedTo);
        if (status.HasValue) query = query.Where(f => f.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search)) { var s = search.ToLower(); query = query.Where(f => f.Subject.ToLower().Contains(s)); }

        var items = query.Select(f => new FollowUpReportItem
        {
            Subject = f.Subject, FollowUpType = f.FollowUpType, FollowUpDate = f.FollowUpDate,
            Status = f.Status.ToString(), AssignedToName = f.AssignedUser.FirstName + " " + f.AssignedUser.LastName,
            RelatedEntity = f.Customer != null ? f.Customer.CustomerName : (f.Lead != null ? f.Lead.LeadName : null),
            IsOverdue = f.Status == FollowUpStatus.Planned && f.FollowUpDate < DateTime.Today
        }).OrderBy(f => f.FollowUpDate);

        return new ReportDataViewModel<FollowUpReportItem> { Items = await PaginatedList<FollowUpReportItem>.CreateAsync(items, page, pageSize) };
    }

    public async Task<ReportDataViewModel<OpportunityReportItem>> GetOpportunityReportAsync(string? search, OpportunityStage? stage, string? assignedTo, int page, int pageSize)
    {
        var query = _context.Opportunities.Include(o => o.Customer).Include(o => o.AssignedUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(o => o.AssignedTo == assignedTo);
        if (stage.HasValue) query = query.Where(o => o.Stage == stage.Value);
        if (!string.IsNullOrWhiteSpace(search)) { var s = search.ToLower(); query = query.Where(o => o.OpportunityName.ToLower().Contains(s)); }

        var items = query.Select(o => new OpportunityReportItem
        {
            OpportunityName = o.OpportunityName, CustomerName = o.Customer != null ? o.Customer.CustomerName : null,
            Stage = o.Stage.ToString(), Amount = o.Amount, Probability = o.Probability,
            WeightedValue = o.Amount * o.Probability / 100, ExpectedCloseDate = o.ExpectedCloseDate,
            AssignedToName = o.AssignedUser.FirstName + " " + o.AssignedUser.LastName
        }).OrderByDescending(o => o.Amount);

        return new ReportDataViewModel<OpportunityReportItem> { Items = await PaginatedList<OpportunityReportItem>.CreateAsync(items, page, pageSize) };
    }

    public async Task<PipelineReportViewModel> GetPipelineReportAsync(string? assignedTo)
    {
        var query = _context.Opportunities.Include(o => o.AssignedUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(o => o.AssignedTo == assignedTo);

        var byStage = await query.GroupBy(o => o.Stage).Select(g => new PipelineStageItem
        {
            Stage = g.Key.ToString(), Count = g.Count(), TotalAmount = g.Sum(o => o.Amount),
            WeightedAmount = g.Sum(o => o.Amount * o.Probability / 100)
        }).ToListAsync();

        var byOwner = await query.GroupBy(o => new { o.AssignedUser.FirstName, o.AssignedUser.LastName }).Select(g => new PipelineOwnerItem
        {
            OwnerName = g.Key.FirstName + " " + g.Key.LastName, Count = g.Count(), TotalAmount = g.Sum(o => o.Amount),
            WeightedAmount = g.Sum(o => o.Amount * o.Probability / 100)
        }).ToListAsync();

        var openQuery = query.Where(o => o.Stage != OpportunityStage.Won && o.Stage != OpportunityStage.Lost);
        return new PipelineReportViewModel
        {
            ByStage = byStage, ByOwner = byOwner,
            TotalPipeline = await openQuery.SumAsync(o => o.Amount),
            WeightedPipeline = await openQuery.SumAsync(o => o.Amount * o.Probability / 100)
        };
    }

    public async Task<ConversionReportViewModel> GetConversionReportAsync(string? assignedTo)
    {
        var query = _context.Leads.AsQueryable();
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(l => l.AssignedTo == assignedTo);

        var total = await query.CountAsync();
        var converted = await query.Where(l => l.Status == LeadStatus.Converted).CountAsync();
        return new ConversionReportViewModel
        {
            TotalLeads = total, ConvertedLeads = converted, NotConvertedLeads = total - converted,
            ConversionRate = total > 0 ? Math.Round((decimal)converted / total * 100, 2) : 0,
            ConversionData = new List<ChartDataItem>
            {
                new() { Label = "Converted", Count = converted },
                new() { Label = "Not Converted", Count = total - converted }
            }
        };
    }

    public async Task<ReportDataViewModel<UserActivityReportItem>> GetUserActivityReportAsync(string? userId, int page, int pageSize)
    {
        var usersQuery = _context.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(userId)) usersQuery = usersQuery.Where(u => u.Id == userId);

        var items = usersQuery.Select(u => new UserActivityReportItem
        {
            UserName = u.FirstName + " " + u.LastName, Email = u.Email!,
            CustomersOwned = _context.Customers.Count(c => c.OwnerId == u.Id),
            LeadsAssigned = _context.Leads.Count(l => l.AssignedTo == u.Id),
            OpportunitiesAssigned = _context.Opportunities.Count(o => o.AssignedTo == u.Id),
            ActivitiesCompleted = _context.Activities.Count(a => a.AssignedTo == u.Id && a.IsCompleted),
            FollowUpsCompleted = _context.FollowUps.Count(f => f.AssignedTo == u.Id && f.Status == FollowUpStatus.Completed)
        }).OrderBy(u => u.UserName);

        return new ReportDataViewModel<UserActivityReportItem> { Items = await PaginatedList<UserActivityReportItem>.CreateAsync(items, page, pageSize) };
    }

    public async Task<ReportDataViewModel<AuditReportItem>> GetAuditReportAsync(string? userId, string? action, string? entity, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _context.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(a => a.UserId == userId);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action.Contains(action));
        if (!string.IsNullOrWhiteSpace(entity)) query = query.Where(a => a.EntityName.Contains(entity));
        if (fromDate.HasValue) query = query.Where(a => a.CreatedDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.CreatedDate <= toDate.Value);

        var items = query.Select(a => new AuditReportItem
        {
            AuditLogId = a.AuditLogId, UserName = a.UserName ?? a.UserId, Action = a.Action,
            EntityName = a.EntityName, RecordId = a.RecordId, Result = a.Result, CreatedDate = a.CreatedDate
        }).OrderByDescending(a => a.CreatedDate);

        return new ReportDataViewModel<AuditReportItem> { Items = await PaginatedList<AuditReportItem>.CreateAsync(items, page, pageSize) };
    }

    public async Task<byte[]> ExportCustomerReportCsvAsync(string? search, CustomerStatus? status, string? ownerId)
    {
        var report = await GetCustomerReportAsync(search, status, ownerId, 1, int.MaxValue);
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Email,Company,Status,Owner,Created Date");
        foreach (var c in report.Items) sb.AppendLine($"\"{c.CustomerCode}\",\"{c.CustomerName}\",\"{c.Email}\",\"{c.CompanyName}\",\"{c.Status}\",\"{c.OwnerName}\",\"{c.CreatedDate:yyyy-MM-dd}\"");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportLeadReportCsvAsync(string? search, LeadStatus? status, string? assignedTo)
    {
        var report = await GetLeadReportAsync(search, status, assignedTo, 1, int.MaxValue);
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Source,Status,Priority,Assigned To,Expected Value,Converted,Created Date");
        foreach (var l in report.Items) sb.AppendLine($"\"{l.LeadCode}\",\"{l.LeadName}\",\"{l.Source}\",\"{l.Status}\",\"{l.Priority}\",\"{l.AssignedToName}\",\"{l.ExpectedValue}\",\"{l.IsConverted}\",\"{l.CreatedDate:yyyy-MM-dd}\"");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportOpportunityReportCsvAsync(string? search, OpportunityStage? stage, string? assignedTo)
    {
        var report = await GetOpportunityReportAsync(search, stage, assignedTo, 1, int.MaxValue);
        var sb = new StringBuilder();
        sb.AppendLine("Name,Customer,Stage,Amount,Probability,Weighted Value,Expected Close,Assigned To");
        foreach (var o in report.Items) sb.AppendLine($"\"{o.OpportunityName}\",\"{o.CustomerName}\",\"{o.Stage}\",\"{o.Amount}\",\"{o.Probability}\",\"{o.WeightedValue}\",\"{o.ExpectedCloseDate:yyyy-MM-dd}\",\"{o.AssignedToName}\"");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
