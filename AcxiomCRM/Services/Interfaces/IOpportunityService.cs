#nullable enable

using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services;

namespace AcxiomCRM.Services.Interfaces;

/// <summary>
/// Service interface for managing sales pipeline opportunities and forecasting metrics.
/// </summary>
public interface IOpportunityService
{
    /// <summary>
    /// Retrieves a paginated list of opportunities based on stage, assigned representative, and search query.
    /// </summary>
    Task<PaginatedList<Opportunity>> GetOpportunitiesAsync(
        string? search,
        OpportunityStage? stage,
        string? assignedTo,
        int page,
        int pageSize,
        string? sortBy,
        bool sortDesc);

    /// <summary>
    /// Retrieves a specific opportunity by its primary key identifier, including related entities.
    /// </summary>
    Task<Opportunity?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new sales opportunity record in the database.
    /// </summary>
    Task<Opportunity> CreateAsync(Opportunity opp);

    /// <summary>
    /// Updates an existing opportunity record.
    /// </summary>
    Task<Opportunity> UpdateAsync(Opportunity opp);

    /// <summary>
    /// Deletes an opportunity by its primary key identifier.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Updates the pipeline stage for an opportunity.
    /// </summary>
    Task<Opportunity> ChangeStageAsync(int id, OpportunityStage newStage);

    /// <summary>
    /// Calculates total open pipeline value (sum of Amount where stage is not Won or Lost).
    /// </summary>
    Task<decimal> GetTotalPipelineValueAsync(string? assignedTo = null);

    /// <summary>
    /// Calculates probability-weighted open pipeline value (sum of Amount * Probability / 100 where stage is not Won or Lost).
    /// </summary>
    Task<decimal> GetWeightedPipelineValueAsync(string? assignedTo = null);

    /// <summary>
    /// Retrieves the count of opportunities matching an optional stage and assigned representative.
    /// </summary>
    Task<int> GetCountByStageAsync(OpportunityStage? stage = null, string? assignedTo = null);
}
