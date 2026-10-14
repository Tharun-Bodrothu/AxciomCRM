#nullable enable
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Models.Enums;
using AcxiomCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Customer>> GetCustomersAsync(string? search, CustomerStatus? status,
        string? ownerId, int page, int pageSize, string? sortBy, bool sortDesc)
    {
        var query = _context.Customers.Include(c => c.Owner).AsQueryable();

        if (!string.IsNullOrWhiteSpace(ownerId))
            query = query.Where(c => c.OwnerId == ownerId);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.CustomerName.ToLower().Contains(s) ||
                c.Email.ToLower().Contains(s) ||
                c.Phone.Contains(s) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(s)));
        }

        query = ApplySort(query, sortBy, sortDesc);
        return await PaginatedList<Customer>.CreateAsync(query, page, pageSize);
    }

    public async Task<Customer?> GetByIdAsync(int id) =>
        await _context.Customers.Include(c => c.Owner).FirstOrDefaultAsync(c => c.CustomerId == id);

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException("A customer with this email or phone already exists.", ex);
        }
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException("A customer with this email or phone already exists.", ex);
        }
        return customer;
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            customer.Status = CustomerStatus.Inactive;
            customer.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var query = _context.Customers.Where(c => c.Email.ToLower() == email.ToLower());
        if (excludeId.HasValue)
            query = query.Where(c => c.CustomerId != excludeId.Value);
        return !await query.AnyAsync();
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId = null)
    {
        var query = _context.Customers.Where(c => c.Phone == phone);
        if (excludeId.HasValue)
            query = query.Where(c => c.CustomerId != excludeId.Value);
        return !await query.AnyAsync();
    }

    public async Task<string> GenerateCustomerCodeAsync()
    {
        var maxCode = await _context.Customers
            .OrderByDescending(c => c.CustomerId)
            .Select(c => c.CustomerCode)
            .FirstOrDefaultAsync();

        if (maxCode != null && maxCode.StartsWith("CUST-"))
        {
            if (int.TryParse(maxCode[5..], out var num))
                return $"CUST-{(num + 1):D5}";
        }
        return "CUST-00001";
    }

    public async Task<int> GetTotalCountAsync(string? ownerId = null)
    {
        var query = _context.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(ownerId))
            query = query.Where(c => c.OwnerId == ownerId);
        return await query.CountAsync();
    }

    private static IQueryable<Customer> ApplySort(IQueryable<Customer> query, string? sortBy, bool desc)
    {
        return sortBy?.ToLower() switch
        {
            "name" => desc ? query.OrderByDescending(c => c.CustomerName) : query.OrderBy(c => c.CustomerName),
            "email" => desc ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "company" => desc ? query.OrderByDescending(c => c.CompanyName) : query.OrderBy(c => c.CompanyName),
            "status" => desc ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "date" => desc ? query.OrderByDescending(c => c.CreatedDate) : query.OrderBy(c => c.CreatedDate),
            _ => query.OrderByDescending(c => c.CreatedDate)
        };
    }
}
