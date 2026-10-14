#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.Models;

/// <summary>
/// Represents a prospective sales lead in AcxiomCRM.
/// </summary>
public class Lead
{
    /// <summary>
    /// Primary key for the lead.
    /// </summary>
    [Key]
    public int LeadId { get; set; }

    /// <summary>
    /// Unique business code identifying the lead (e.g. LEAD-0001).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string LeadCode { get; set; } = string.Empty;

    /// <summary>
    /// Full lead name or primary contact name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string LeadName { get; set; } = string.Empty;

    /// <summary>
    /// Lead contact email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Lead contact phone number.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Company or organization name of the lead.
    /// </summary>
    [MaxLength(200)]
    public string? CompanyName { get; set; }

    /// <summary>
    /// Acquisition source of the lead (e.g. Website, Referral, Campaign).
    /// </summary>
    [MaxLength(100)]
    public string? Source { get; set; }

    /// <summary>
    /// Current lifecycle status of the lead.
    /// </summary>
    [Required]
    public LeadStatus Status { get; set; } = LeadStatus.New;

    /// <summary>
    /// Priority level assigned to the lead.
    /// </summary>
    [Required]
    public LeadPriority Priority { get; set; } = LeadPriority.Medium;

    /// <summary>
    /// Estimated commercial value if converted.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExpectedValue { get; set; }

    /// <summary>
    /// General notes or remarks regarding the lead.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Foreign key identifier of the user assigned to this lead.
    /// </summary>
    [Required]
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key of the customer record created upon conversion, if converted.
    /// </summary>
    public int? ConvertedCustomerId { get; set; }

    /// <summary>
    /// Date and time when the lead was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the lead record was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Identifier or username of the user who created the record.
    /// </summary>
    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Application user assigned to handle this lead.
    /// </summary>
    [ForeignKey(nameof(AssignedTo))]
    public virtual ApplicationUser AssignedUser { get; set; } = null!;

    /// <summary>
    /// Customer entity resulting from conversion of this lead, if applicable.
    /// </summary>
    [ForeignKey(nameof(ConvertedCustomerId))]
    public virtual Customer? ConvertedCustomer { get; set; }

    /// <summary>
    /// Opportunities generated from this lead.
    /// </summary>
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();

    /// <summary>
    /// Follow-ups scheduled for this lead.
    /// </summary>
    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();

    /// <summary>
    /// Activities logged against this lead.
    /// </summary>
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
