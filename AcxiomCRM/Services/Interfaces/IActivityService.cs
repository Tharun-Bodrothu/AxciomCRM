#nullable enable
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services;

namespace AcxiomCRM.Services.Interfaces;

public interface IActivityService
{
    Task<PaginatedList<Activity>> GetActivitiesAsync(string? search, ActivityType? type, string? assignedTo, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? sortBy, bool sortDesc);
    Task<Activity?> GetByIdAsync(int id);
    Task<Activity> CreateAsync(Activity activity);
    Task<Activity> UpdateAsync(Activity activity);
    Task DeleteAsync(int id);
}
