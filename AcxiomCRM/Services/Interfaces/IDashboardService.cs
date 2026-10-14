#nullable enable
using AcxiomCRM.ViewModels;

namespace AcxiomCRM.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync(string userId, string role, DateTime? startDate, DateTime? endDate);
}
