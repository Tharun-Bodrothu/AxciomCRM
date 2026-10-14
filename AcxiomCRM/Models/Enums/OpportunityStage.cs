#nullable enable

namespace AcxiomCRM.Models.Enums;

/// <summary>
/// Defines the sales pipeline stages for an opportunity.
/// </summary>
public enum OpportunityStage
{
    /// <summary>
    /// Initial qualification of opportunity scope and budget.
    /// </summary>
    Qualification = 1,

    /// <summary>
    /// Proposal submitted to the client.
    /// </summary>
    Proposal = 2,

    /// <summary>
    /// Terms, pricing, or contract under active negotiation.
    /// </summary>
    Negotiation = 3,

    /// <summary>
    /// Opportunity successfully won and closed.
    /// </summary>
    Won = 4,

    /// <summary>
    /// Opportunity lost or cancelled.
    /// </summary>
    Lost = 5
}
