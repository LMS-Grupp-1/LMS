using Domain.Models.Entities;
using LMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations
{
	public sealed class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
	{
		public void Configure(EntityTypeBuilder<Feedback> builder)
		{
			builder.Property(feedback => feedback.Comment)
				.IsRequired()
				.HasMaxLength(AssignmentConstraints.FeedbackCommentMaxLength);

			// At most one feedback per submission.
			builder.HasOne(feedback => feedback.Submission)
				.WithOne(submission => submission.Feedback)
				.HasForeignKey<Feedback>(feedback => feedback.SubmissionId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(feedback => feedback.SubmissionId)
				.IsUnique();

			builder.HasOne(feedback => feedback.Teacher)
				.WithMany()
				.HasForeignKey(feedback => feedback.TeacherId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
