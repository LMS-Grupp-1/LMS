using Domain.Models.Entities;
using LMS.Shared.DTOs.AssignmentDtos;

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
				latest?.Feedback is not null);
		}
	}
}
