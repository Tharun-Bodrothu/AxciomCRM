#nullable enable

using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="Lead"/>.
/// </summary>
public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");

        builder.HasKey(l => l.LeadId);

        builder.Property(l => l.LeadCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.LeadName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.Phone)
            .HasMaxLength(20);

        builder.Property(l => l.CompanyName)
            .HasMaxLength(200);

        builder.Property(l => l.Source)
            .HasMaxLength(100);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.ExpectedValue)
            .HasPrecision(18, 2);

        builder.Property(l => l.Notes)
            .HasMaxLength(2000);

        builder.Property(l => l.AssignedTo)
            .IsRequired();

        builder.Property(l => l.CreatedDate)
            .IsRequired();

        builder.Property(l => l.CreatedBy)
            .HasMaxLength(256);

        // Unique index on LeadCode
        builder.HasIndex(l => l.LeadCode)
            .IsUnique();

        // Foreign Key to ApplicationUser (AssignedTo) with Restrict
        builder.HasOne(l => l.AssignedUser)
            .WithMany(u => u.Leads)
            .HasForeignKey(l => l.AssignedTo)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign Key to Customer (ConvertedCustomerId) with SetNull
        builder.HasOne(l => l.ConvertedCustomer)
            .WithMany(c => c.Leads)
            .HasForeignKey(l => l.ConvertedCustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
