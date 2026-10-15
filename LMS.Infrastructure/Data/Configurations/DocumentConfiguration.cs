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

            builder.HasIndex(document => document.Name)
                .IsUnique();

            builder.HasIndex(document => document.Description)
                .IsUnique();

            builder.Property(document => document.BlobPath)
                .HasMaxLength(450)
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
                    "CK_Documents_OneTarget",
                    "[SizeBytes] > 0 AND [SizeBytes] <= 10458760");

            });
        }

    }
}
