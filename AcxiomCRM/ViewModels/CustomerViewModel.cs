#nullable enable
using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.ViewModels;

public class CustomerViewModel
{
    public int CustomerId { get; set; }
    public string? CustomerCode { get; set; }

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters.")]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required.")]
    [Phone(ErrorMessage = "Invalid phone format.")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Company Name")]
    public string? CompanyName { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    [Display(Name = "Owner")]
    public string? OwnerId { get; set; }
    public string? OwnerName { get; set; }
}
