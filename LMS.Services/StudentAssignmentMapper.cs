using Domain.Models.Entities;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.FeedbackDtos;

namespace LMS.Services
{	
	internal static class StudentAssignmentMapper
	{
		public static StudentAssignmentDto ToDto(Assignment assignment, DateTimeOffset now)
		{
			// If the student has handed in more than once, the latest submission counts.
			var latest = assignment.Submissions
				.OrderByDescending(submission => submission.SubmittedAt)
				.FirstOrDefault();

			DateTimeOffset? deadline = assignment.Deadline is { } localDeadline
				? AssignmentStatusCalculator.ToSwedishTime(localDeadline)
				: null;

            var feedbackDtos = latest?.Feedbacks?
                .Select(f => new FeedbackForStudentDto(
                    f.Id,
                    f.Teacher != null ? $"{f.Teacher.FirstName} {f.Teacher.LastName}" : "Unknown Teacher",
                    f.Comment,
                    f.CreatedAt
                ))
                .ToList() ?? new List<FeedbackForStudentDto>();

            return new StudentAssignmentDto(
				assignment.Id,
				assignment.Name,
				assignment.Description,
				assignment.Deadline,
				assignment.ModuleId,
				assignment.Module.Name,
				assignment.RequiresSubmission,
				AssignmentStatusCalculator.Calculate(
					assignment.RequiresSubmission, deadline, latest?.SubmittedAt, now),
				latest?.SubmittedAt,
                feedbackDtos.Any(),
				feedbackDtos);
		}
	}
}
