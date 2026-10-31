using Domain.Models.Entities;
using LMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations
{
    public sealed class SharedDocumentConfiguration : IEntityTypeConfiguration<SharedDocument>
    {
        public void Configure(EntityTypeBuilder<SharedDocument> builder)
        {
            builder.Property(sd => sd.FileName)
                .HasMaxLength(DocumentConstraints.MaxFileNameLength)
                .IsRequired();

            builder.Property(sd => sd.ContentType)
                .HasMaxLength(DocumentConstraints.MaxContentTypeLength)
                .IsRequired();

            builder.Property(sd => sd.Name)
                .HasMaxLength(DocumentConstraints.MaxNameLength)
                .IsRequired();

            builder.Property(sd => sd.Description)
                .HasMaxLength(DocumentConstraints.MaxDescriptionLength)
                .IsRequired();

            builder.Property(sd => sd.BlobPath)
                .HasMaxLength(DocumentConstraints.MaxBlopPathLength)
                .IsRequired();

            builder.HasIndex(sd => sd.BlobPath)
                .IsUnique();

            builder.HasOne(sd => sd.UploadedBy)
                .WithMany()
                .HasForeignKey(sd => sd.UploadedById)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sd => sd.Course)
                .WithMany(c => c.SharedDocuments)
                .HasForeignKey(sd => sd.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sd => sd.Module)
                .WithMany(m => m.SharedDocuments)
                .HasForeignKey(sd => sd.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

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
