#nullable enable

namespace AcxiomCRM.Models.Enums;

/// <summary>
/// Defines the lifecycle status of a customer record.
/// </summary>
public enum CustomerStatus
{
    /// <summary>
    /// Active customer with ongoing business engagement.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Inactive customer with no recent engagement.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Prospective customer undergoing evaluation.
    /// </summary>
    Prospect = 3,

    /// <summary>
    /// Customer that has ceased doing business.
    /// </summary>
    Churned = 4
}
