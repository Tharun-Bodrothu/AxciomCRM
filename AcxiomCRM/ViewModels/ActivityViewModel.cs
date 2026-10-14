#nullable enable
using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.ViewModels;

public class ActivityViewModel
{
    public int ActivityId { get; set; }

    [Required]
    public ActivityType Type { get; set; } = ActivityType.Call;

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Activity date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Activity Date")]
    public DateTime ActivityDate { get; set; } = DateTime.Today;

    [Display(Name = "Customer")]
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }

    [Display(Name = "Lead")]
    public int? LeadId { get; set; }
    public string? LeadName { get; set; }

    [Display(Name = "Assigned To")]
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }

    [Display(Name = "Completed")]
    public bool IsCompleted { get; set; }
}
