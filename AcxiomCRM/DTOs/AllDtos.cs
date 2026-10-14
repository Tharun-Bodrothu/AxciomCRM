#nullable enable
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.DTOs;

// ── Customer DTOs ──────────────────────────────────────────────────────
public class CustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class CreateCustomerDto
{
    [Required] public string CustomerName { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    [Required][Phone] public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? OwnerId { get; set; }
}

// ── Lead DTOs ──────────────────────────────────────────────────────────
public class LeadDto
{
    public int LeadId { get; set; }
    public string LeadCode { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Source { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public decimal? ExpectedValue { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class CreateLeadDto
{
    [Required] public string LeadName { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? Source { get; set; }
    public string? Priority { get; set; }
    public decimal? ExpectedValue { get; set; }
    public string? AssignedTo { get; set; }
    public string? Notes { get; set; }
}

// ── Opportunity DTOs ───────────────────────────────────────────────────
public class OpportunityDto
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string Stage { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Probability { get; set; }
    public decimal WeightedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
}

public class CreateOpportunityDto
{
    [Required] public string OpportunityName { get; set; } = string.Empty;
    [Required][Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    [Required][Range(0, 100)] public decimal Probability { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? Notes { get; set; }
    public string? AssignedTo { get; set; }
}

// ── API Response ───────────────────────────────────────────────────────
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

public class StatusChangeDto
{
    [Required] public string Status { get; set; } = string.Empty;
}

public class LeadConvertDto
{
    [Required] public string OpportunityName { get; set; } = string.Empty;
    [Required][Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
}

public class StageChangeDto
{
    [Required] public string Stage { get; set; } = string.Empty;
}
