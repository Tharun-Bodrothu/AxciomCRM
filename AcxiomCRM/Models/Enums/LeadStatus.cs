#nullable enable

namespace AcxiomCRM.Models.Enums;

/// <summary>
/// Defines the lifecycle stages of a sales lead.
/// </summary>
public enum LeadStatus
{
    /// <summary>
    /// Newly created lead awaiting initial contact.
    /// </summary>
    New = 1,

    /// <summary>
    /// Initial contact has been established with the lead.
    /// </summary>
    Contacted = 2,

    /// <summary>
    /// Lead has been qualified as a viable sales opportunity.
    /// </summary>
    Qualified = 3,

    /// <summary>
    /// Lead does not meet qualification criteria.
    /// </summary>
    Unqualified = 4,

    /// <summary>
    /// Lead has been successfully converted into a customer.
    /// </summary>
    Converted = 5,

    /// <summary>
    /// Lead engagement concluded without conversion.
    /// </summary>
    Lost = 6
}
