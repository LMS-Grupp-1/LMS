using Domain.Contracts;
using Domain.Models.Entities;
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

		public async Task<IReadOnlyList<TeacherAssignmentDto>> GetForModuleAsync(int moduleId)
		{
			// 404 instead of an empty list when the module does not exist.
			_ = await _uow.Modules.GetByIdAsync(moduleId)
				?? throw new ModuleNotFoundException(moduleId);

			return await _uow.Assignments.GetForModuleAsync(moduleId);
		}

		public async Task CreateAsync(AssignmentFormDto dto)
		{
			var activity = await GetValidatedActivityAsync(dto);

			var assignment = new Assignment
			{				
				ModuleId = activity.ModuleId,
				ActivityId = activity.Id
			};
			ApplyForm(assignment, dto);

			await _uow.Assignments.AddAsync(assignment);
			await _uow.CompleteAsync();
		}

		public async Task UpdateAsync(int id, AssignmentFormDto dto)
		{
			// GetByIdAsync uses FindAsync, so the entity is tracked and changes are saved by CompleteAsync.
			var assignment = await _uow.Assignments.GetByIdAsync(id)
				?? throw new AssignmentNotFoundException();

			var activity = await GetValidatedActivityAsync(dto);

			// Moving is allowed, but only within the same module.
			if (activity.ModuleId != assignment.ModuleId)
			{
				throw new BadRequestException("Uppgiften kan bara flyttas till en aktivitet i samma modul.");
			}

			assignment.ActivityId = activity.Id;
			ApplyForm(assignment, dto);

			await _uow.CompleteAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var assignment = await _uow.Assignments.GetByIdAsync(id)
				?? throw new AssignmentNotFoundException();

			if (await _uow.Assignments.HasSubmissionsAsync(id))
			{
				throw new AssignmentHasSubmissionsException();
			}

			_uow.Assignments.Delete(assignment);
			await _uow.CompleteAsync();
		}

		// Shared rules for create and update. Returns the activity with module and course loaded.
		private async Task<Activity> GetValidatedActivityAsync(AssignmentFormDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Name))
			{
				throw new BadRequestException("Namn krävs.");
			}

			var activity = await _uow.Activities.GetWithModuleAndCourseAsync(dto.ActivityId)
				?? throw new EntityNotFoundException("Aktivitet", dto.ActivityId);

			// After the module end is allowed (the client shows a warning), after the course end is not.
			// Compare dates only, since the course end date is stored as midnight.
			var courseEnd = activity.Module.Course.EndDate;
			if (dto.Deadline.HasValue && dto.Deadline.Value.Date > courseEnd.Date)
			{
				throw new BadRequestException(
					$"Deadline får inte ligga efter kursens slutdatum ({courseEnd:yyyy-MM-dd}).");
			}

			return activity;
		}

		// Copies the editable fields from the form to the entity.
		private static void ApplyForm(Assignment assignment, AssignmentFormDto dto)
		{
			assignment.Name = dto.Name.Trim();
			assignment.Description = dto.Description?.Trim() ?? string.Empty;
			assignment.Deadline = dto.Deadline;
			assignment.RequiresSubmission = dto.RequiresSubmission;
		}
	}
}
