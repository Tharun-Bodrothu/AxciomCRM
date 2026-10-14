#nullable enable

using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="FollowUp"/>.
/// </summary>
public class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FollowUp> builder)
    {
        builder.ToTable("FollowUps");

        builder.HasKey(f => f.FollowUpId);

        builder.Property(f => f.FollowUpDate)
            .IsRequired();

        builder.Property(f => f.FollowUpType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Remarks)
            .HasMaxLength(2000);

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(f => f.AssignedTo)
            .IsRequired();

        builder.Property(f => f.CreatedDate)
            .IsRequired();

        // Foreign Key to Customer with SetNull
        builder.HasOne(f => f.Customer)
            .WithMany(c => c.FollowUps)
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Foreign Key to Lead with SetNull
        builder.HasOne(f => f.Lead)
            .WithMany(l => l.FollowUps)
            .HasForeignKey(f => f.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Foreign Key to Opportunity with SetNull
        builder.HasOne(f => f.Opportunity)
            .WithMany(o => o.FollowUps)
            .HasForeignKey(f => f.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        // Foreign Key to ApplicationUser (AssignedTo) with Restrict
        builder.HasOne(f => f.AssignedUser)
            .WithMany(u => u.FollowUps)
            .HasForeignKey(f => f.AssignedTo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
