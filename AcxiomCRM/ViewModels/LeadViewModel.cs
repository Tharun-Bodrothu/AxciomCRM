#nullable enable
using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.ViewModels;

public class LeadViewModel
{
    public int LeadId { get; set; }
    public string? LeadCode { get; set; }

    [Required(ErrorMessage = "Lead name is required.")]
    [StringLength(200)]
    [Display(Name = "Lead Name")]
    public string LeadName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone format.")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(200)]
    [Display(Name = "Company Name")]
    public string? CompanyName { get; set; }

    [StringLength(100)]
    public string? Source { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadPriority Priority { get; set; } = LeadPriority.Medium;

    [Display(Name = "Expected Value")]
    [Range(0, double.MaxValue, ErrorMessage = "Expected value must be non-negative.")]
    public decimal? ExpectedValue { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Display(Name = "Assigned To")]
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
}

public class LeadConvertViewModel
{
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "Opportunity name is required.")]
    [StringLength(200)]
    [Display(Name = "Opportunity Name")]
    public string OpportunityName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Display(Name = "Assigned To")]
    public string? AssignedTo { get; set; }
}
