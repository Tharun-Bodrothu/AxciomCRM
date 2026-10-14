#nullable enable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AcxiomCRM.Models;

/// <summary>
/// Represents an authenticated system user extending ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full display name combining first and last names.
    /// </summary>
    public string? FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Indicates whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when the user account was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the user last logged in.
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Customers owned by this user.
    /// </summary>
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    /// <summary>
    /// Leads assigned to this user.
    /// </summary>
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    /// <summary>
    /// Opportunities assigned to this user.
    /// </summary>
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();

    /// <summary>
    /// Follow-ups assigned to this user.
    /// </summary>
    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();

    /// <summary>
    /// Activities assigned to this user.
    /// </summary>
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
