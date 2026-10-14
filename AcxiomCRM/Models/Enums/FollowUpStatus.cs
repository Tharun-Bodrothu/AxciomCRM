#nullable enable

namespace AcxiomCRM.Models.Enums;

/// <summary>
/// Defines the status of a scheduled follow-up interaction.
/// </summary>
public enum FollowUpStatus
{
    /// <summary>
    /// Follow-up is planned and scheduled for future execution.
    /// </summary>
    Planned = 1,

    /// <summary>
    /// Follow-up has been successfully completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Follow-up scheduled time has passed without completion.
    /// </summary>
    Missed = 3,

    /// <summary>
    /// Follow-up was cancelled prior to execution.
    /// </summary>
    Cancelled = 4
}
