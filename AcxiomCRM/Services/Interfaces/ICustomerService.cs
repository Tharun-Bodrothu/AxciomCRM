#nullable enable

using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services;

namespace AcxiomCRM.Services.Interfaces;

/// <summary>
/// Service interface for managing customer accounts and associated business operations.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Retrieves a paginated list of customers filtered by search term, status, and owner.
    /// </summary>
    Task<PaginatedList<Customer>> GetCustomersAsync(
        string? search,
        CustomerStatus? status,
        string? ownerId,
        int page,
        int pageSize,
        string? sortBy,
        bool sortDesc);

    /// <summary>
    /// Retrieves a single customer by its unique primary key identifier.
    /// </summary>
    Task<Customer?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new customer entity in the database.
    /// </summary>
    Task<Customer> CreateAsync(Customer customer);

    /// <summary>
    /// Updates an existing customer entity.
    /// </summary>
    Task<Customer> UpdateAsync(Customer customer);

    /// <summary>
    /// Deletes or deactivates a customer by its primary key identifier.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks whether an email address is unique among customers, optionally excluding a specific record.
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);

    /// <summary>
    /// Checks whether a phone number is unique among customers, optionally excluding a specific record.
    /// </summary>
    Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId = null);

    /// <summary>
    /// Generates a sequential, formatted customer code (e.g. CUST-00001).
    /// </summary>
    Task<string> GenerateCustomerCodeAsync();

    /// <summary>
    /// Retrieves the total count of customers, optionally filtered by owner identifier.
    /// </summary>
    Task<int> GetTotalCountAsync(string? ownerId = null);
}
