#nullable enable
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services.Implementations;

public class OpportunityService : IOpportunityService
{
    private readonly ApplicationDbContext _context;

    public OpportunityService(ApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<Opportunity>> GetOpportunitiesAsync(string? search, OpportunityStage? stage,
        string? assignedTo, int page, int pageSize, string? sortBy, bool sortDesc)
    {
        var query = _context.Opportunities.Include(o => o.Customer).Include(o => o.AssignedUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(o => o.AssignedTo == assignedTo);
        if (stage.HasValue) query = query.Where(o => o.Stage == stage.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.OpportunityName.ToLower().Contains(s) ||
                (o.Customer != null && o.Customer.CustomerName.ToLower().Contains(s)));
        }

        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc ? query.OrderByDescending(o => o.OpportunityName) : query.OrderBy(o => o.OpportunityName),
            "amount" => sortDesc ? query.OrderByDescending(o => o.Amount) : query.OrderBy(o => o.Amount),
            "stage" => sortDesc ? query.OrderByDescending(o => o.Stage) : query.OrderBy(o => o.Stage),
            "probability" => sortDesc ? query.OrderByDescending(o => o.Probability) : query.OrderBy(o => o.Probability),
            "closedate" => sortDesc ? query.OrderByDescending(o => o.ExpectedCloseDate) : query.OrderBy(o => o.ExpectedCloseDate),
            _ => query.OrderByDescending(o => o.CreatedDate)
        };

        return await PaginatedList<Opportunity>.CreateAsync(query, page, pageSize);
    }

    public async Task<Opportunity?> GetByIdAsync(int id) =>
        await _context.Opportunities.Include(o => o.Customer).Include(o => o.Lead).Include(o => o.AssignedUser)
            .FirstOrDefaultAsync(o => o.OpportunityId == id);

    public async Task<Opportunity> CreateAsync(Opportunity opp)
    {
        _context.Opportunities.Add(opp);
        await _context.SaveChangesAsync();
        return opp;
    }

    public async Task<Opportunity> UpdateAsync(Opportunity opp)
    {
        _context.Opportunities.Update(opp);
        await _context.SaveChangesAsync();
        return opp;
    }

    public async Task DeleteAsync(int id)
    {
        var opp = await _context.Opportunities.FindAsync(id);
        if (opp != null) { _context.Opportunities.Remove(opp); await _context.SaveChangesAsync(); }
    }

    public async Task<Opportunity> ChangeStageAsync(int id, OpportunityStage newStage)
    {
        var opp = await _context.Opportunities.FindAsync(id) ?? throw new KeyNotFoundException($"Opportunity {id} not found.");
        opp.Stage = newStage;
        if (newStage == OpportunityStage.Won) opp.Probability = 100;
        if (newStage == OpportunityStage.Lost) opp.Probability = 0;
        opp.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return opp;
    }

    public async Task<decimal> GetTotalPipelineValueAsync(string? assignedTo = null)
    {
        var query = _context.Opportunities.Where(o => o.Stage != OpportunityStage.Won && o.Stage != OpportunityStage.Lost);
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(o => o.AssignedTo == assignedTo);
        return await query.SumAsync(o => o.Amount);
    }

    public async Task<decimal> GetWeightedPipelineValueAsync(string? assignedTo = null)
    {
        var query = _context.Opportunities.Where(o => o.Stage != OpportunityStage.Won && o.Stage != OpportunityStage.Lost);
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(o => o.AssignedTo == assignedTo);
        return await query.SumAsync(o => o.Amount * o.Probability / 100);
    }

    public async Task<int> GetCountByStageAsync(OpportunityStage? stage = null, string? assignedTo = null)
    {
        var query = _context.Opportunities.AsQueryable();
        if (stage.HasValue) query = query.Where(o => o.Stage == stage.Value);
        if (!string.IsNullOrWhiteSpace(assignedTo)) query = query.Where(o => o.AssignedTo == assignedTo);
        return await query.CountAsync();
    }
}
