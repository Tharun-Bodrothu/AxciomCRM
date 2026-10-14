#nullable enable
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services;

namespace AcxiomCRM.Services.Interfaces;

public interface ILeadService
{
    Task<PaginatedList<Lead>> GetLeadsAsync(string? search, LeadStatus? status, LeadPriority? priority, string? assignedTo, int page, int pageSize, string? sortBy, bool sortDesc);
    Task<Lead?> GetByIdAsync(int id);
    Task<Lead> CreateAsync(Lead lead);
    Task<Lead> UpdateAsync(Lead lead);
    Task DeleteAsync(int id);
    Task<Lead> ChangeStatusAsync(int id, LeadStatus newStatus);
    Task<(Customer customer, Opportunity opportunity)> ConvertLeadAsync(int leadId, string opportunityName, decimal amount, string assignedTo);
    Task<bool> IsValidStatusTransition(LeadStatus current, LeadStatus target);
    Task<string> GenerateLeadCodeAsync();
    Task<int> GetCountByStatusAsync(LeadStatus? status = null, string? assignedTo = null);
}
