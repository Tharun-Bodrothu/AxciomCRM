#nullable enable

using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="Opportunity"/>.
/// </summary>
public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("Opportunities");

        builder.HasKey(o => o.OpportunityId);

        builder.Property(o => o.OpportunityName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.Probability)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(o => o.Stage)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.Notes)
            .HasMaxLength(2000);

        builder.Property(o => o.AssignedTo)
            .IsRequired();

        builder.Property(o => o.CreatedDate)
            .IsRequired();

        builder.Property(o => o.CreatedBy)
            .HasMaxLength(256);

        // Ignore computed property
        builder.Ignore(o => o.WeightedValue);

        // Foreign Key to Customer with Restrict
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Opportunities)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign Key to Lead with Restrict
        builder.HasOne(o => o.Lead)
            .WithMany(l => l.Opportunities)
            .HasForeignKey(o => o.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign Key to ApplicationUser (AssignedTo) with Restrict
        builder.HasOne(o => o.AssignedUser)
            .WithMany(u => u.Opportunities)
            .HasForeignKey(o => o.AssignedTo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
