#nullable enable
using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.ViewModels;

public class FollowUpViewModel
{
    public int FollowUpId { get; set; }

    [Display(Name = "Customer")]
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }

    [Display(Name = "Lead")]
    public int? LeadId { get; set; }
    public string? LeadName { get; set; }

    [Display(Name = "Opportunity")]
    public int? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }

    [Required(ErrorMessage = "Follow-up date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Follow-Up Date")]
    public DateTime FollowUpDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Follow-up type is required.")]
    [StringLength(50)]
    [Display(Name = "Type")]
    public string FollowUpType { get; set; } = "Call";

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Remarks { get; set; }

    public FollowUpStatus Status { get; set; } = FollowUpStatus.Planned;

    [Display(Name = "Assigned To")]
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
}
