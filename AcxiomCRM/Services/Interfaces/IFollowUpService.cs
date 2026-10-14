#nullable enable
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services;

namespace AcxiomCRM.Services.Interfaces;

public interface IFollowUpService
{
    Task<PaginatedList<FollowUp>> GetFollowUpsAsync(string? search, FollowUpStatus? status, string? assignedTo, DateTime? fromDate, DateTime? toDate, int page, int pageSize, string? sortBy, bool sortDesc);
    Task<FollowUp?> GetByIdAsync(int id);
    Task<FollowUp> CreateAsync(FollowUp followUp);
    Task<FollowUp> UpdateAsync(FollowUp followUp);
    Task<FollowUp> CompleteAsync(int id, string? remarks);
    Task<FollowUp> RescheduleAsync(int id, DateTime newDate, string? remarks);
    Task<FollowUp> CancelAsync(int id, string? remarks);
    Task<List<FollowUp>> GetUpcomingAsync(string? assignedTo, int days = 7);
    Task<List<FollowUp>> GetOverdueAsync(string? assignedTo);
}
