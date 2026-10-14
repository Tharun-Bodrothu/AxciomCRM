#nullable enable

using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="Activity"/>.
/// </summary>
public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");

        builder.HasKey(a => a.ActivityId);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.ActivityDate)
            .IsRequired();

        builder.Property(a => a.AssignedTo)
            .IsRequired();

        builder.Property(a => a.IsCompleted)
            .IsRequired();

        builder.Property(a => a.CreatedDate)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(256);

        // Foreign Key to Customer with SetNull
        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Activities)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Foreign Key to Lead with SetNull
        builder.HasOne(a => a.Lead)
            .WithMany(l => l.Activities)
            .HasForeignKey(a => a.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Foreign Key to ApplicationUser (AssignedTo) with Restrict
        builder.HasOne(a => a.AssignedUser)
            .WithMany(u => u.Activities)
            .HasForeignKey(a => a.AssignedTo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
