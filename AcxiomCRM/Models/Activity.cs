#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.Models;

/// <summary>
/// Represents an activity (Call, Meeting, Email, Task) performed in AcxiomCRM.
/// </summary>
public class Activity
{
    /// <summary>
    /// Primary key for the activity.
    /// </summary>
    [Key]
    public int ActivityId { get; set; }

    /// <summary>
    /// Type of activity (Call, Meeting, Email, Task).
    /// </summary>
    [Required]
    public ActivityType Type { get; set; } = ActivityType.Call;

    /// <summary>
    /// Title or subject of the activity.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the activity content or agenda.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Scheduled or occurrence date and time of the activity.
    /// </summary>
    [Required]
    public DateTime ActivityDate { get; set; }

    /// <summary>
    /// Optional foreign key to an associated customer.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Optional foreign key to an associated lead.
    /// </summary>
    public int? LeadId { get; set; }

    /// <summary>
    /// Foreign key identifier of the user assigned to this activity.
    /// </summary>
    [Required]
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the activity has been completed.
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Date and time when the activity was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the activity record was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Identifier or username of the user who created the record.
    /// </summary>
    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Customer associated with this activity, if applicable.
    /// </summary>
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Lead associated with this activity, if applicable.
    /// </summary>
    [ForeignKey(nameof(LeadId))]
    public virtual Lead? Lead { get; set; }

    /// <summary>
    /// Application user assigned to this activity.
    /// </summary>
    [ForeignKey(nameof(AssignedTo))]
    public virtual ApplicationUser AssignedUser { get; set; } = null!;
}
