#nullable enable
using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.ViewModels;

public class OpportunityViewModel
{
    public int OpportunityId { get; set; }

    [Required(ErrorMessage = "Opportunity name is required.")]
    [StringLength(200)]
    [Display(Name = "Opportunity Name")]
    public string OpportunityName { get; set; } = string.Empty;

    [Display(Name = "Customer")]
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }

    [Display(Name = "Lead")]
    public int? LeadId { get; set; }
    public string? LeadName { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public OpportunityStage Stage { get; set; } = OpportunityStage.Qualification;

    [Required(ErrorMessage = "Probability is required.")]
    [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
    public decimal Probability { get; set; }

    [Display(Name = "Expected Close Date")]
    [DataType(DataType.Date)]
    public DateTime? ExpectedCloseDate { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Display(Name = "Assigned To")]
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }

    public decimal WeightedValue => Amount * Probability / 100;
}
