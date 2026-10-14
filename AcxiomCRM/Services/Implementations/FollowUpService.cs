#nullable enable
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services.Implementations;

public class FollowUpService : IFollowUpService
{
    private readonly ApplicationDbContext _context;

    public FollowUpService(ApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<FollowUp>> GetFollowUpsAsync(string? search, FollowUpStatus? status,
        string? assignedTo, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? sortBy, bool sortDesc)
    {
        var query = _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.AssignedUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(f => f.AssignedTo == assignedTo);
        if (status.HasValue) query = query.Where(f => f.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(f => f.FollowUpDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(f => f.FollowUpDate <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(f => f.Subject.ToLower().Contains(s) || f.FollowUpType.ToLower().Contains(s));
        }

        query = sortBy?.ToLower() switch
        {
            "subject" => sortDesc ? query.OrderByDescending(f => f.Subject) : query.OrderBy(f => f.Subject),
            "status" => sortDesc ? query.OrderByDescending(f => f.Status) : query.OrderBy(f => f.Status),
            "type" => sortDesc ? query.OrderByDescending(f => f.FollowUpType) : query.OrderBy(f => f.FollowUpType),
            _ => query.OrderBy(f => f.FollowUpDate)
        };

        return await PaginatedList<FollowUp>.CreateAsync(query, page, pageSize);
    }

    public async Task<FollowUp?> GetByIdAsync(int id) =>
        await _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead)
            .Include(f => f.Opportunity).Include(f => f.AssignedUser)
            .FirstOrDefaultAsync(f => f.FollowUpId == id);

    public async Task<FollowUp> CreateAsync(FollowUp followUp)
    {
        _context.FollowUps.Add(followUp);
        await _context.SaveChangesAsync();
        return followUp;
    }

    public async Task<FollowUp> UpdateAsync(FollowUp followUp)
    {
        _context.FollowUps.Update(followUp);
        await _context.SaveChangesAsync();
        return followUp;
    }

    public async Task<FollowUp> CompleteAsync(int id, string? remarks)
    {
        var f = await _context.FollowUps.FindAsync(id) ?? throw new KeyNotFoundException($"FollowUp {id} not found.");
        f.Status = FollowUpStatus.Completed;
        f.Remarks = remarks ?? f.Remarks;
        f.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return f;
    }

    public async Task<FollowUp> RescheduleAsync(int id, DateTime newDate, string? remarks)
    {
        if (newDate.Date < DateTime.Today)
            throw new InvalidOperationException("Rescheduled date cannot be in the past.");

        var f = await _context.FollowUps.FindAsync(id) ?? throw new KeyNotFoundException($"FollowUp {id} not found.");
        f.FollowUpDate = newDate;
        f.Status = FollowUpStatus.Planned;
        f.Remarks = remarks ?? f.Remarks;
        f.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return f;
    }

    public async Task<FollowUp> CancelAsync(int id, string? remarks)
    {
        var f = await _context.FollowUps.FindAsync(id) ?? throw new KeyNotFoundException($"FollowUp {id} not found.");
        f.Status = FollowUpStatus.Cancelled;
        f.Remarks = remarks ?? f.Remarks;
        f.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return f;
    }

    public async Task<List<FollowUp>> GetUpcomingAsync(string? assignedTo, int days = 7)
    {
        var query = _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.AssignedUser)
            .Where(f => f.Status == FollowUpStatus.Planned && f.FollowUpDate >= DateTime.Today && f.FollowUpDate <= DateTime.Today.AddDays(days));
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(f => f.AssignedTo == assignedTo);
        return await query.OrderBy(f => f.FollowUpDate).ToListAsync();
    }

    public async Task<List<FollowUp>> GetOverdueAsync(string? assignedTo)
    {
        var query = _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.AssignedUser)
            .Where(f => f.Status == FollowUpStatus.Planned && f.FollowUpDate < DateTime.Today);
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(f => f.AssignedTo == assignedTo);
        return await query.OrderBy(f => f.FollowUpDate).ToListAsync();
    }
}
