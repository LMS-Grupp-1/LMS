using System;
using System.Collections.Generic;
using System.Text;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations
{
    public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.Property(document => document.FileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(document => document.ContentType)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(document => document.StorageKey)
                .HasMaxLength(64)
                .IsRequired();

            builder.HasIndex(document => document.StorageKey)
                .IsUnique();

            builder.Property(document => document.UploadedByName)
                .HasMaxLength(300)
                .IsRequired();

            builder.HasOne(document => document.UploadedBy)
                .WithMany()
                .HasForeignKey(document => document.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(document => document.Course)
                .WithMany()
                .HasForeignKey(document => document.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(document => document.Module)
                .WithMany()
                .HasForeignKey(document => document.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(document => document.Activity)
                .WithMany()
                .HasForeignKey(document => document.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(table =>
            {
                table.HasCheckConstraint( // Ensures that each doc belongs to at least 1 course, module or activity.
                    "CK_Documents_OneTarget",
                    """
                    (
                        CASE WHEN [CourseId] IS NULL THEN 0 ELSE 1 END +
                        CASE WHEN [ModuleId] IS NULL THEN 0 ELSE 1 END +
                        CASE WHEN [ActivityId] IS NULL THEN 0 ELSE 1 END
                    ) = 1
                    """);

                table.HasCheckConstraint(
                    "CK_Documents_Size",
                    "[SizeBytes] > 0 AND [SizeBytes] <= 10485760");

            });
        }

    }
}
