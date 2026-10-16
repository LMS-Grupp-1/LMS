using Domain.Models.Entities;
using LMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infrastructure.Data.Configurations
{
    public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.Property(document => document.FileName)
                .HasMaxLength(DocumentConstraints.MaxFileNameLength)
                .IsRequired();

            builder.Property(document => document.ContentType)
                .HasMaxLength(DocumentConstraints.MaxContentTypeLength)
                .IsRequired();

            builder.Property(document => document.Name)
                .HasMaxLength(DocumentConstraints.MaxNameLength)
                .IsRequired();

            builder.Property(document => document.Description)
                .HasMaxLength(DocumentConstraints.MaxDescriptionLength)
                .IsRequired();

            builder.Property(document => document.BlobPath)
                .HasMaxLength(DocumentConstraints.MaxBlopPathLength)
                .IsRequired();

            builder.HasIndex(document => document.BlobPath)
                .IsUnique();

            builder.HasOne(document => document.UploadedBy)
                .WithMany()
                .HasForeignKey(document => document.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(document => document.Course)
                .WithMany(course => course.Documents)
                .HasForeignKey(document => document.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(document => document.Module)
                .WithMany(module => module.Documents)
                .HasForeignKey(document => document.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(document => document.Activity)
                .WithMany(activity => activity.Documents)
                .HasForeignKey(document => document.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Documents_Size",
                    FormattableString.Invariant(
                    $"[SizeBytes] > 0 AND [SizeBytes] <= {DocumentConstraints.MaxFileSize}"));

            });
        }

    }
}
