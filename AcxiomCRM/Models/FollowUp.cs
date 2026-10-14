#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.Models;

/// <summary>
/// Represents a scheduled or completed follow-up interaction.
/// </summary>
public class FollowUp
{
    /// <summary>
    /// Primary key for the follow-up record.
    /// </summary>
    [Key]
    public int FollowUpId { get; set; }

    /// <summary>
    /// Optional foreign key to an associated customer.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Optional foreign key to an associated lead.
    /// </summary>
    public int? LeadId { get; set; }

    /// <summary>
    /// Optional foreign key to an associated opportunity.
    /// </summary>
    public int? OpportunityId { get; set; }

    /// <summary>
    /// Scheduled date and time for the follow-up.
    /// </summary>
    [Required]
    public DateTime FollowUpDate { get; set; }

    /// <summary>
    /// Type of interaction (e.g. Call, Email, Meeting, Visit). Max length 50.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string FollowUpType { get; set; } = string.Empty;

    /// <summary>
    /// Subject or objective of the follow-up.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Detailed notes, outcome, or remarks from the follow-up.
    /// </summary>
    [MaxLength(2000)]
    public string? Remarks { get; set; }

    /// <summary>
    /// Current status of the follow-up (Planned, Completed, Missed, Cancelled).
    /// </summary>
    [Required]
    public FollowUpStatus Status { get; set; } = FollowUpStatus.Planned;

    /// <summary>
    /// Foreign key identifier of the user assigned to execute this follow-up.
    /// </summary>
    [Required]
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the follow-up was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the follow-up record was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Customer associated with this follow-up, if applicable.
    /// </summary>
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Lead associated with this follow-up, if applicable.
    /// </summary>
    [ForeignKey(nameof(LeadId))]
    public virtual Lead? Lead { get; set; }

    /// <summary>
    /// Opportunity associated with this follow-up, if applicable.
    /// </summary>
    [ForeignKey(nameof(OpportunityId))]
    public virtual Opportunity? Opportunity { get; set; }

    /// <summary>
    /// Application user assigned to perform this follow-up.
    /// </summary>
    [ForeignKey(nameof(AssignedTo))]
    public virtual ApplicationUser AssignedUser { get; set; } = null!;
}
