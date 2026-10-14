using Domain.Models.Entities;
using LMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                   .IsRequired()
                   .HasMaxLength(ModuleConstraints.NameMaxLength);

            builder.Property(m => m.Description)
                   .HasMaxLength(ModuleConstraints.DescriptionMaxLength);

            // A module belongs to one course. 
            // If the course is deleted, cascade delete its modules.
            builder.HasOne(m => m.Course)
                   .WithMany(c => c.Modules)
                   .HasForeignKey(m => m.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}