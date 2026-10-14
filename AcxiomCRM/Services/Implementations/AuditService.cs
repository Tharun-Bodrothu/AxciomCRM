#nullable enable
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string userId, string userName, string action, string entityName,
        string? recordId, string? oldValue, string? newValue, string? ipAddress,
        string? result = "Success", string? details = null)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityName = entityName,
                RecordId = recordId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = ipAddress,
                Result = result,
                AdditionalDetails = details,
                CreatedDate = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log: {Action} on {Entity} by {User}", action, entityName, userName);
        }
    }

    public async Task LogLoginAsync(string userId, string userName, bool success, string? ipAddress, string? details = null)
    {
        await LogAsync(userId, userName,
            success ? "LoginSuccess" : "LoginFailed",
            "Authentication", null, null, null, ipAddress,
            success ? "Success" : "Failed", details);
    }

    public async Task<PaginatedList<AuditLog>> GetAuditLogsAsync(string? userId, string? action, string? entity,
        DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(a => a.UserId == userId);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action.Contains(action));
        if (!string.IsNullOrWhiteSpace(entity)) query = query.Where(a => a.EntityName.Contains(entity));
        if (from.HasValue) query = query.Where(a => a.CreatedDate >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedDate <= to.Value);

        query = query.OrderByDescending(a => a.CreatedDate);
        return await PaginatedList<AuditLog>.CreateAsync(query, page, pageSize);
    }
}
