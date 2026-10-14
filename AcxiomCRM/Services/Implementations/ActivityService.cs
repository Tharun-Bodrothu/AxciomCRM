#nullable enable
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly ApplicationDbContext _context;

    public ActivityService(ApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<Activity>> GetActivitiesAsync(string? search, ActivityType? type,
        string? assignedTo, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? sortBy, bool sortDesc)
    {
        var query = _context.Activities.Include(a => a.Customer).Include(a => a.Lead).Include(a => a.AssignedUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(a => a.AssignedTo == assignedTo);
        if (type.HasValue) query = query.Where(a => a.Type == type.Value);
        if (fromDate.HasValue) query = query.Where(a => a.ActivityDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.ActivityDate <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(a => a.Subject.ToLower().Contains(s) || (a.Description != null && a.Description.ToLower().Contains(s)));
        }

        query = sortBy?.ToLower() switch
        {
            "subject" => sortDesc ? query.OrderByDescending(a => a.Subject) : query.OrderBy(a => a.Subject),
            "type" => sortDesc ? query.OrderByDescending(a => a.Type) : query.OrderBy(a => a.Type),
            _ => query.OrderByDescending(a => a.ActivityDate)
        };

        return await PaginatedList<Activity>.CreateAsync(query, page, pageSize);
    }

    public async Task<Activity?> GetByIdAsync(int id) =>
        await _context.Activities.Include(a => a.Customer).Include(a => a.Lead).Include(a => a.AssignedUser)
            .FirstOrDefaultAsync(a => a.ActivityId == id);

    public async Task<Activity> CreateAsync(Activity activity)
    {
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task<Activity> UpdateAsync(Activity activity)
    {
        _context.Activities.Update(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task DeleteAsync(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity != null) { _context.Activities.Remove(activity); await _context.SaveChangesAsync(); }
    }
}
