#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.Models;

/// <summary>
/// Represents a commercial sales opportunity in AcxiomCRM.
/// </summary>
public class Opportunity
{
    /// <summary>
    /// Primary key for the opportunity.
    /// </summary>
    [Key]
    public int OpportunityId { get; set; }

    /// <summary>
    /// Name or title of the sales opportunity.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string OpportunityName { get; set; } = string.Empty;

    /// <summary>
    /// Optional foreign key to an existing customer account.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Optional foreign key to the originating lead.
    /// </summary>
    public int? LeadId { get; set; }

    /// <summary>
    /// Total monetary amount or contract value of the opportunity.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Current pipeline stage of the opportunity.
    /// </summary>
    [Required]
    public OpportunityStage Stage { get; set; } = OpportunityStage.Qualification;

    /// <summary>
    /// Win probability percentage (0 to 100).
    /// </summary>
    [Required]
    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Probability { get; set; }

    /// <summary>
    /// Projected close date for the opportunity.
    /// </summary>
    public DateTime? ExpectedCloseDate { get; set; }

    /// <summary>
    /// Additional opportunity notes and specifications.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Foreign key identifier of the user assigned to this opportunity.
    /// </summary>
    [Required]
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the opportunity was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the opportunity record was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Identifier or username of the user who created the record.
    /// </summary>
    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Computed expected value based on amount multiplied by win probability percentage.
    /// </summary>
    [NotMapped]
    public decimal WeightedValue => Math.Round(Amount * Probability / 100m, 2);

    /// <summary>
    /// Customer account associated with this opportunity, if linked.
    /// </summary>
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Lead associated with this opportunity, if linked.
    /// </summary>
    [ForeignKey(nameof(LeadId))]
    public virtual Lead? Lead { get; set; }

    /// <summary>
    /// Application user assigned to manage this opportunity.
    /// </summary>
    [ForeignKey(nameof(AssignedTo))]
    public virtual ApplicationUser AssignedUser { get; set; } = null!;

    /// <summary>
    /// Follow-ups scheduled for this opportunity.
    /// </summary>
    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
}
