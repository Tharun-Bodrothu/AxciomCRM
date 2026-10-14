#nullable enable

namespace AcxiomCRM.Models.Enums;

/// <summary>
/// Defines the category of CRM activity logged.
/// </summary>
public enum ActivityType
{
    /// <summary>
    /// Telephonic voice conversation.
    /// </summary>
    Call = 1,

    /// <summary>
    /// In-person or virtual video conference.
    /// </summary>
    Meeting = 2,

    /// <summary>
    /// Electronic mail correspondence.
    /// </summary>
    Email = 3,

    /// <summary>
    /// Internal or external action item / task.
    /// </summary>
    Task = 4
}
