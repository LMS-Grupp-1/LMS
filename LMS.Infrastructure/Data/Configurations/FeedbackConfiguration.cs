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

            // A submission have many feedbacks.
            builder.HasOne(feedback => feedback.Submission)
            .WithMany(submission => submission.Feedbacks)
            .HasForeignKey(feedback => feedback.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(feedback => feedback.Teacher)
                .WithMany()
                .HasForeignKey(feedback => feedback.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
	}
}
