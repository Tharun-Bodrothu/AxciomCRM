#nullable enable

using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data;

/// <summary>
/// Entity Framework Core database context for AcxiomCRM.
/// Integrates ASP.NET Core Identity with CRM business domain models.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <param name="options">Database context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Customers DbSet.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Gets or sets the Leads DbSet.
    /// </summary>
    public DbSet<Lead> Leads => Set<Lead>();

    /// <summary>
    /// Gets or sets the Opportunities DbSet.
    /// </summary>
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();

    /// <summary>
    /// Gets or sets the FollowUps DbSet.
    /// </summary>
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();

    /// <summary>
    /// Gets or sets the Activities DbSet.
    /// </summary>
    public DbSet<Activity> Activities => Set<Activity>();

    /// <summary>
    /// Gets or sets the AuditLogs DbSet.
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations in the current assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Explicit unique index configuration
        builder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        builder.Entity<Customer>()
            .HasIndex(c => c.Phone)
            .IsUnique();

        builder.Entity<Customer>()
            .HasIndex(c => c.CustomerCode)
            .IsUnique();

        builder.Entity<Lead>()
            .HasIndex(l => l.LeadCode)
            .IsUnique();
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        UpdateAuditTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Automatically populates CreatedDate and ModifiedDate timestamps on entity state changes.
    /// </summary>
    private void UpdateAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                var createdDateProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedDate");
                if (createdDateProp != null && createdDateProp.CurrentValue is DateTime dt && dt == default)
                {
                    createdDateProp.CurrentValue = utcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var modifiedDateProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "ModifiedDate");
                if (modifiedDateProp != null)
                {
                    modifiedDateProp.CurrentValue = utcNow;
                }
            }
        }
    }
}
