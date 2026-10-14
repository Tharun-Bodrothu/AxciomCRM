#nullable enable
using AcxiomCRM.Models;
using AcxiomCRM.Services;

namespace AcxiomCRM.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string userId, string userName, string action, string entityName, string? recordId, string? oldValue, string? newValue, string? ipAddress, string? result = "Success", string? details = null);
    Task LogLoginAsync(string userId, string userName, bool success, string? ipAddress, string? details = null);
    Task<PaginatedList<AuditLog>> GetAuditLogsAsync(string? userId, string? action, string? entity, DateTime? from, DateTime? to, int page, int pageSize);
}
