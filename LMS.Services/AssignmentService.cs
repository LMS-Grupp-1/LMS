using Domain.Contracts;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.AssignmentDtos;
using Service.Contracts;

namespace LMS.Services
{
	public class AssignmentService(IUnitOfWork uow, TimeProvider timeProvider) : IAssignmentService
	{
		private readonly IUnitOfWork _uow = uow;
		private readonly TimeProvider _timeProvider = timeProvider;

		public async Task<IReadOnlyList<StudentAssignmentDto>> GetMyAssignmentsAsync(string userId)
		{
			// The course comes from the logged-in user, never from the request,
			// so a student can only see assignments in their own course.
			var course = await _uow.Courses.GetForStudentAsync(userId, trackChanges: false)
				?? throw new StudentCourseNotFoundException();

			var assignments = await _uow.Assignments.GetForStudentAsync(course.Id, userId);
			var now = _timeProvider.GetUtcNow();

			return assignments
				.Select(assignment =>
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
				})
				.ToList();
		}
	}
}
