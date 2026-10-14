#nullable enable

using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="Customer"/>.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.CustomerId);

        builder.Property(c => c.CustomerCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CompanyName)
            .HasMaxLength(200);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.City)
            .HasMaxLength(100);

        builder.Property(c => c.State)
            .HasMaxLength(100);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.OwnerId)
            .IsRequired();

        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(256);

        // Unique indexes
        builder.HasIndex(c => c.CustomerCode)
            .IsUnique();

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.HasIndex(c => c.Phone)
            .IsUnique();

        // Foreign Key to ApplicationUser (OwnerId) with Restrict
        builder.HasOne(c => c.Owner)
            .WithMany(u => u.Customers)
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
