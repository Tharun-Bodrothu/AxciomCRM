#nullable enable
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services.Implementations;

public class LeadService : ILeadService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeadService> _logger;

    // Valid status transitions
    private static readonly Dictionary<LeadStatus, LeadStatus[]> ValidTransitions = new()
    {
        { LeadStatus.New, new[] { LeadStatus.Contacted, LeadStatus.Unqualified, LeadStatus.Lost } },
        { LeadStatus.Contacted, new[] { LeadStatus.Qualified, LeadStatus.Unqualified, LeadStatus.Lost } },
        { LeadStatus.Qualified, new[] { LeadStatus.Converted, LeadStatus.Lost } },
        { LeadStatus.Unqualified, Array.Empty<LeadStatus>() },
        { LeadStatus.Converted, Array.Empty<LeadStatus>() },
        { LeadStatus.Lost, Array.Empty<LeadStatus>() }
    };

    public LeadService(ApplicationDbContext context, ILogger<LeadService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Lead>> GetLeadsAsync(string? search, LeadStatus? status, LeadPriority? priority,
        string? assignedTo, int page, int pageSize, string? sortBy, bool sortDesc)
    {
        var query = _context.Leads.Include(l => l.AssignedUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(assignedTo))
            query = query.Where(l => l.AssignedTo == assignedTo);
        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);
        if (priority.HasValue)
            query = query.Where(l => l.Priority == priority.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(l =>
                l.LeadName.ToLower().Contains(s) ||
                l.Email.ToLower().Contains(s) ||
                (l.CompanyName != null && l.CompanyName.ToLower().Contains(s)));
        }

        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc ? query.OrderByDescending(l => l.LeadName) : query.OrderBy(l => l.LeadName),
            "status" => sortDesc ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
            "priority" => sortDesc ? query.OrderByDescending(l => l.Priority) : query.OrderBy(l => l.Priority),
            "value" => sortDesc ? query.OrderByDescending(l => l.ExpectedValue) : query.OrderBy(l => l.ExpectedValue),
            _ => query.OrderByDescending(l => l.CreatedDate)
        };

        return await PaginatedList<Lead>.CreateAsync(query, page, pageSize);
    }

    public async Task<Lead?> GetByIdAsync(int id) =>
        await _context.Leads.Include(l => l.AssignedUser).Include(l => l.ConvertedCustomer)
            .FirstOrDefaultAsync(l => l.LeadId == id);

    public async Task<Lead> CreateAsync(Lead lead)
    {
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();
        return lead;
    }

    public async Task<Lead> UpdateAsync(Lead lead)
    {
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync();
        return lead;
    }

    public async Task DeleteAsync(int id)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead != null)
        {
            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Lead> ChangeStatusAsync(int id, LeadStatus newStatus)
    {
        var lead = await _context.Leads.FindAsync(id)
            ?? throw new KeyNotFoundException($"Lead {id} not found.");

        if (!await IsValidStatusTransition(lead.Status, newStatus))
            throw new InvalidOperationException($"Cannot transition from {lead.Status} to {newStatus}.");

        lead.Status = newStatus;
        lead.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return lead;
    }

    public Task<bool> IsValidStatusTransition(LeadStatus current, LeadStatus target)
    {
        if (ValidTransitions.TryGetValue(current, out var allowed))
            return Task.FromResult(allowed.Contains(target));
        return Task.FromResult(false);
    }

    public async Task<(Customer customer, Opportunity opportunity)> ConvertLeadAsync(
        int leadId, string opportunityName, decimal amount, string assignedTo)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var lead = await _context.Leads.FindAsync(leadId)
                ?? throw new KeyNotFoundException($"Lead {leadId} not found.");

            if (lead.Status != LeadStatus.Qualified)
                throw new InvalidOperationException("Only qualified leads can be converted.");

            // Check for existing customer with same email to avoid duplicates
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email.ToLower() == lead.Email.ToLower());

            Customer customer;
            if (existingCustomer != null)
            {
                customer = existingCustomer;
            }
            else
            {
                var customerCode = await GenerateCustomerCodeForConversion();
                customer = new Customer
                {
                    CustomerCode = customerCode,
                    CustomerName = lead.LeadName,
                    Email = lead.Email,
                    Phone = lead.Phone ?? "",
                    CompanyName = lead.CompanyName,
                    Status = CustomerStatus.Active,
                    OwnerId = assignedTo,
                    CreatedBy = assignedTo,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            var opportunity = new Opportunity
            {
                OpportunityName = opportunityName,
                CustomerId = customer.CustomerId,
                LeadId = leadId,
                Amount = amount,
                Stage = OpportunityStage.Qualification,
                Probability = 20,
                AssignedTo = assignedTo,
                CreatedBy = assignedTo,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            lead.Status = LeadStatus.Converted;
            lead.ConvertedCustomerId = customer.CustomerId;
            lead.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Lead {LeadId} converted to Customer {CustomerId} and Opportunity {OpportunityId}.",
                leadId, customer.CustomerId, opportunity.OpportunityId);

            return (customer, opportunity);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> GenerateLeadCodeAsync()
    {
        var maxCode = await _context.Leads.OrderByDescending(l => l.LeadId).Select(l => l.LeadCode).FirstOrDefaultAsync();
        if (maxCode != null && maxCode.StartsWith("LEAD-") && int.TryParse(maxCode[5..], out var num))
            return $"LEAD-{(num + 1):D5}";
        return "LEAD-00001";
    }

    public async Task<int> GetCountByStatusAsync(LeadStatus? status = null, string? assignedTo = null)
    {
        var query = _context.Leads.AsQueryable();
        if (status.HasValue) query = query.Where(l => l.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(l => l.AssignedTo == assignedTo);
        return await query.CountAsync();
    }

    private async Task<string> GenerateCustomerCodeForConversion()
    {
        var maxCode = await _context.Customers.OrderByDescending(c => c.CustomerId).Select(c => c.CustomerCode).FirstOrDefaultAsync();
        if (maxCode != null && maxCode.StartsWith("CUST-") && int.TryParse(maxCode[5..], out var num))
            return $"CUST-{(num + 1):D5}";
        return "CUST-00001";
    }
}
