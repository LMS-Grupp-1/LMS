using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations;

public class CourseInvitationConfiguration : IEntityTypeConfiguration<CourseInvitation>
{
    public void Configure(EntityTypeBuilder<CourseInvitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Token)
            .IsRequired()
            .HasMaxLength(128);

        // Create an index for fast lookups when verifying tokens
        builder.HasIndex(i => i.Token).IsUnique();
        builder.HasIndex(i => i.Email);
    }
}
