#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcxiomCRM.Models;

/// <summary>
/// Records security and entity mutation audit trail entries.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Primary key for the audit log record.
    /// </summary>
    [Key]
    public long AuditLogId { get; set; }

    /// <summary>
    /// Identifier of the user who performed the action.
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Username or email of the user who performed the action.
    /// </summary>
    [MaxLength(256)]
    public string? UserName { get; set; }

    /// <summary>
    /// Action executed (e.g. Create, Update, Delete, Login, Logout).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Entity or component targeted (e.g. Customer, Lead, Opportunity).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the record modified.
    /// </summary>
    [MaxLength(100)]
    public string? RecordId { get; set; }

    /// <summary>
    /// JSON or string representation of state prior to modification.
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? OldValue { get; set; }

    /// <summary>
    /// JSON or string representation of state following modification.
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? NewValue { get; set; }

    /// <summary>
    /// Client IP address from which the request originated.
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Result outcome of the action (e.g. Success, Failed, Unauthorized).
    /// </summary>
    [MaxLength(100)]
    public string? Result { get; set; }

    /// <summary>
    /// Optional diagnostic or context details.
    /// </summary>
    [MaxLength(2000)]
    public string? AdditionalDetails { get; set; }

    /// <summary>
    /// Date and time when the audit event occurred.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
