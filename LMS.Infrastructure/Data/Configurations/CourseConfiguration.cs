using Domain.Models.Entities;
using LMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations
{
	public class CourseConfiguration : IEntityTypeConfiguration<Course>
	{
		public void Configure(EntityTypeBuilder<Course> builder)
		{
			builder.HasKey(c => c.Id);

			builder.Property(c => c.Name)
				   .IsRequired()
				   .HasMaxLength(CourseConstraints.NameMaxLength);

			builder.Property(c => c.Description)
				   .HasMaxLength(CourseConstraints.DescriptionMaxLength);

			// Extra safety net in the database. The service layer validates this first
			builder.ToTable(t => t.HasCheckConstraint(
				"CK_Course_StartBeforeEnd", "[StartDate] < [EndDate]"));

			// A course has many students, a student belongs to at most one course.
			// Restrict: a course with students cannot be deleted by accident
			builder.HasMany(c => c.Students)
				   .WithOne(u => u.Course)
				   .HasForeignKey(u => u.CourseId)
				   .OnDelete(DeleteBehavior.Restrict);
		}
	}
}
