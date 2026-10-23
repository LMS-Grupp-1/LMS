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
			var course = await _uow.Courses.GetForStudentAsync(userId, trackChanges: false)
				?? throw new StudentCourseNotFoundException();

			var assignments = await _uow.Assignments.GetForStudentAsync(course.Id, userId);
			var now = _timeProvider.GetUtcNow();

			return assignments
			.Select(assignment => StudentAssignmentMapper.ToDto(assignment, now))
			.ToList();
		}
	}
}
