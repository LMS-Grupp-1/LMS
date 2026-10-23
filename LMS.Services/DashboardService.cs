using Domain.Contracts;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.DashboardDtos;
using Service.Contracts;

namespace LMS.Services
{
	// Puts together the student dashboard from existing data: course, modules,
	// activities, assignments and documents. No data of its own.
	public sealed class DashboardService(IUnitOfWork uow, TimeProvider timeProvider) : IDashboardService
	{
		private const int MaxActivities = 5;
		private const int MaxDeadlines = 5;
		private const int MaxFeedback = 3;
		private const int MaxDocuments = 5;

		public async Task<StudentDashboardDto> GetStudentDashboardAsync(
			string userId,
			CancellationToken cancellationToken)
		{
			// The course comes from the logged-in user, never from the request,
			// so the dashboard only shows the student's own course.
			var course = await uow.Courses.GetForStudentAsync(userId, trackChanges: false)
				?? throw new StudentCourseNotFoundException();

			// UTC for submission statuses; Swedish local time for stored dates and times.
			var nowUtc = timeProvider.GetUtcNow();
			var nowSwedish = AssignmentStatusCalculator.ToSwedishLocalTime(nowUtc);
			var today = nowSwedish.Date;

			// The module running today. Compared by date, since module dates have no time of day.
			var modules = await uow.Modules.GetModulesByCourseIdAsync(course.Id);
			var currentModule = modules
				.Where(module => module.StartDate.Date <= today && today <= module.EndDate.Date)
				.Select(module => new DashboardModuleDto(module.Id, module.Name, module.StartDate, module.EndDate))
				.FirstOrDefault();

			var activities = await uow.Activities.GetUpcomingByCourseIdAsync(
				course.Id, nowSwedish, MaxActivities, cancellationToken);

			// The same mapping and status as "My assignments", so both views always agree.
			var assignments = (await uow.Assignments.GetForStudentAsync(course.Id, userId))
				.Select(assignment => StudentAssignmentMapper.ToDto(assignment, nowUtc))
				.ToList();

			// Deadline passed and nothing handed in: what needs to be done now.
			var overdue = assignments
				.Where(assignment => assignment.Status == AssignmentStatus.Late && assignment.SubmittedAt is null)
				.ToList();

			// Deadline still ahead. The repository already orders by deadline.
			var upcomingDeadlines = assignments
				.Where(assignment => assignment.Deadline is { } deadline && deadline >= nowSwedish)
				.Take(MaxDeadlines)
				.ToList();

			// The most recently handed-in assignments that have feedback.
			var withFeedback = assignments
				.Where(assignment => assignment.HasFeedback)
				.OrderByDescending(assignment => assignment.SubmittedAt)
				.Take(MaxFeedback)
				.ToList();

			// The document list is already ordered with the newest first.
			var documents = (await uow.Documents.ListAsync(course.Id, cancellationToken))
				.Take(MaxDocuments)
				.ToList();

			return new StudentDashboardDto(
				new DashboardCourseDto(course.Id, course.Name, course.StartDate, course.EndDate),
				currentModule,
				activities,
				overdue,
				upcomingDeadlines,
				withFeedback,
				documents);
		}
	}
}
