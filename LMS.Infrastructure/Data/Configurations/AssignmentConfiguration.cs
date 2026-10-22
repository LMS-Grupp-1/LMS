using Domain.Models.Entities;
using LMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations
{
	public sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
	{
		public void Configure(EntityTypeBuilder<Assignment> builder)
		{
			builder.Property(assignment => assignment.Name)
				.IsRequired()
				.HasMaxLength(AssignmentConstraints.NameMaxLength);

			builder.Property(assignment => assignment.Description)
				.HasMaxLength(AssignmentConstraints.DescriptionMaxLength);

			// Same as activities: a module with assignments cannot be deleted by accident.
			builder.HasOne(assignment => assignment.Module)
				.WithMany(module => module.Assignments)
				.HasForeignKey(assignment => assignment.ModuleId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
