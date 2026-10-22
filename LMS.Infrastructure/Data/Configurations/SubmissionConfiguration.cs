using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations
{
	public sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
	{
		public void Configure(EntityTypeBuilder<Submission> builder)
		{
			// Restrict everywhere: a student's handed-in work is never deleted as a side effect.
			builder.HasOne(submission => submission.Assignment)
				.WithMany(assignment => assignment.Submissions)
				.HasForeignKey(submission => submission.AssignmentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(submission => submission.Student)
				.WithMany()
				.HasForeignKey(submission => submission.StudentId)
				.OnDelete(DeleteBehavior.Restrict);

			// One file belongs to one submission.
			builder.HasOne(submission => submission.Document)
				.WithOne()
				.HasForeignKey<Submission>(submission => submission.DocumentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(submission => submission.DocumentId)
				.IsUnique();

			// Status lookups always filter on assignment and student.
			builder.HasIndex(submission => new { submission.AssignmentId, submission.StudentId });
		}
	}
}
