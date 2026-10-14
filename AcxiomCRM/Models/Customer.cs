#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.Models;

/// <summary>
/// Represents a customer account record in AcxiomCRM.
/// </summary>
public class Customer
{
    /// <summary>
    /// Primary key for the customer.
    /// </summary>
    [Key]
    public int CustomerId { get; set; }

    /// <summary>
    /// Unique business code identifying the customer (e.g. CUST-0001).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>
    /// Full customer name or primary contact display name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Unique primary contact email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Unique primary contact phone number.
    /// </summary>
    [Required]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Organization or company name associated with the customer.
    /// </summary>
    [MaxLength(200)]
    public string? CompanyName { get; set; }

    /// <summary>
    /// Street address.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// City of address.
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// State or region of address.
    /// </summary>
    [MaxLength(100)]
    public string? State { get; set; }

    /// <summary>
    /// Current lifecycle status of the customer.
    /// </summary>
    [Required]
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    /// <summary>
    /// Foreign key identifier of the user who owns this customer record.
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the record was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the record was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Identifier or username of the user who created the record.
    /// </summary>
    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Application user who owns this customer record.
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ApplicationUser Owner { get; set; } = null!;

    /// <summary>
    /// Leads converted to this customer account.
    /// </summary>
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    /// <summary>
    /// Sales opportunities associated with this customer.
    /// </summary>
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();

    /// <summary>
    /// Follow-up actions scheduled for this customer.
    /// </summary>
    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();

    /// <summary>
    /// Activities logged against this customer.
    /// </summary>
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
