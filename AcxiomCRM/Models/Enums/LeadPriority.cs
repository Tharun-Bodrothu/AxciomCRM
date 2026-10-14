#nullable enable

namespace AcxiomCRM.Models.Enums;

/// <summary>
/// Defines the priority levels assigned to sales leads.
/// </summary>
public enum LeadPriority
{
    /// <summary>
    /// Low priority lead.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium / standard priority lead.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High priority lead requiring accelerated engagement.
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical urgency lead requiring immediate attention.
    /// </summary>
    Critical = 4
}
