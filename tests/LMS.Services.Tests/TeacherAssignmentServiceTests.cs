using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using Domain.Models.Exceptions.AssignmentExceptions;
using Domain.Models.Exceptions.ModuleExceptions;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.Users;
using Moq;

namespace LMS.Services.Tests
{
	/// <summary>
	/// Unit tests for the teacher methods in <see cref="AssignmentService"/>:
	/// list, create, update (including moving) and delete.
	/// </summary>
	/// <remarks>
	/// The repositories are mocks, so no database is used.
	/// Setup(...) decides what a mock returns, and Verify(...) checks what the service called.
	///
	/// Not covered here:
	/// - The projection in AssignmentRepository.GetForModuleAsync, which needs a real database.
	/// - The warning for a deadline after the module end, which is shown in the Blazor client only.
	/// </remarks>
	public class TeacherAssignmentServiceTests
	{
		// Shared test data: module 3 runs 2–13 November and belongs to a course ending 18 December.
		private const int ModuleId = 3;
		private const int ActivityId = 10;
		private static readonly DateTime ModuleEnd = new(2026, 11, 13);
		private static readonly DateTime CourseEnd = new(2026, 12, 18);

		// The mocks. Each one replaces a real repository.
		private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
		private readonly Mock<IActivityRepository> _activityRepository = new();
		private readonly Mock<IModuleRepository> _moduleRepository = new();
		private readonly Mock<IUnitOfWork> _uow = new();

		// "System under test": the real service, which uses the mocks above.
		private readonly AssignmentService _sut;

		public TeacherAssignmentServiceTests()
		{
			// The service reaches the repositories through the unit of work.
			_uow.Setup(u => u.Assignments).Returns(_assignmentRepository.Object);
			_uow.Setup(u => u.Activities).Returns(_activityRepository.Object);
			_uow.Setup(u => u.Modules).Returns(_moduleRepository.Object);

			// The teacher methods never read the clock, so the real one is fine.
			_sut = new AssignmentService(_uow.Object, TimeProvider.System);
		}

		/// <summary>
		/// Scenario: the teacher creates an assignment on an existing activity.
		/// Expected: the assignment is saved with the module taken from the activity.
		/// </summary>
		[Fact]
		public async Task CreateAsync_ValidForm_SavesAssignmentWithModuleFromActivity()
		{
			// Arrange: the activity exists in module 3.
			SetupActivity(CreateActivity(ActivityId, ModuleId));
			var form = CreateForm(deadline: new DateTime(2026, 11, 10, 17, 0, 0));

            // Mock student lookup for the new notification logic so it returns an empty list safely
            _uow.Setup(u => u.Users.GetUsersByCourseIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<UserLookupDto>());

            // Mock Notifications AddAsync to succeed when student notifications are registered
            _uow.Setup(u => u.Notifications.AddAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.CreateAsync(form);

			// Assert: the saved assignment has the module and activity from the server side,
			// and the name is trimmed.
			_assignmentRepository.Verify(r => r.AddAsync(It.Is<Assignment>(a =>
				a.ModuleId == ModuleId &&
				a.ActivityId == ActivityId &&
				a.Name == "Konsolapp" &&
				a.RequiresSubmission)), Times.Once);
			_uow.Verify(u => u.CompleteAsync(), Times.Once);
		}

		/// <summary>
		/// Scenario: the form points to an activity that does not exist.
		/// Expected: 404 (EntityNotFoundException) and nothing is saved.
		/// </summary>
		[Fact]
		public async Task CreateAsync_ActivityNotFound_ThrowsAndSavesNothing()
		{
			// Arrange: the repository finds no activity.
			_activityRepository
				.Setup(r => r.GetWithModuleAndCourseAsync(ActivityId))
				.ReturnsAsync((Activity?)null);

			// Act & Assert
			await Assert.ThrowsAsync<EntityNotFoundException>(
				() => _sut.CreateAsync(CreateForm()));
			_uow.Verify(u => u.CompleteAsync(), Times.Never);
		}

		/// <summary>
		/// Scenario: the deadline is the day after the course ends.
		/// Expected: 400 (BadRequestException) and nothing is saved.
		/// </summary>
		[Fact]
		public async Task CreateAsync_DeadlineAfterCourseEnd_ThrowsBadRequest()
		{
			SetupActivity(CreateActivity(ActivityId, ModuleId));
			var form = CreateForm(deadline: CourseEnd.AddDays(1).AddHours(17));

			await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateAsync(form));
			_uow.Verify(u => u.CompleteAsync(), Times.Never);
		}

		/// <summary>
		/// Scenario: the deadline is 17:00 on the last day of the course.
		/// Expected: allowed. The course end date is stored as midnight,
		/// so this checks that only the date is compared.
		/// </summary>
		[Fact]
		public async Task CreateAsync_DeadlineOnLastCourseDayAfternoon_IsAllowed()
		{
			SetupActivity(CreateActivity(ActivityId, ModuleId));
			var form = CreateForm(deadline: CourseEnd.AddHours(17));

            // Mock student lookup for the new notification logic so it returns an empty list safely
            _uow.Setup(u => u.Users.GetUsersByCourseIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<UserLookupDto>());

            // Mock Notifications AddAsync to succeed when student notifications are registered
            _uow.Setup(u => u.Notifications.AddAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            await _sut.CreateAsync(form);

			_uow.Verify(u => u.CompleteAsync(), Times.Once);
		}

		/// <summary>
		/// Scenario: the deadline is after the module ends but before the course ends.
		/// Expected: allowed. Only the client shows a warning for this.
		/// </summary>
		[Fact]
		public async Task CreateAsync_DeadlineAfterModuleEndWithinCourse_IsAllowed()
		{
			SetupActivity(CreateActivity(ActivityId, ModuleId));
			var form = CreateForm(deadline: ModuleEnd.AddDays(5).AddHours(17));

            // Mock student lookup for the new notification logic so it returns an empty list safely
            _uow.Setup(u => u.Users.GetUsersByCourseIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<UserLookupDto>());

            // Mock Notifications AddAsync to succeed when student notifications are registered
            _uow.Setup(u => u.Notifications.AddAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            await _sut.CreateAsync(form);

			_uow.Verify(u => u.CompleteAsync(), Times.Once);
		}

		/// <summary>
		/// Scenario: the teacher moves an assignment to another activity in the same module.
		/// Expected: the activity id is changed and saved.
		/// </summary>
		[Fact]
		public async Task UpdateAsync_ActivityInSameModule_MovesAssignment()
		{
			// Arrange: the assignment belongs to activity 10, the teacher picks activity 11 in the same module.
			var assignment = new Assignment { Id = 1, ModuleId = ModuleId, ActivityId = ActivityId, Name = "Gammalt namn" };
			_assignmentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(assignment);
			SetupActivity(CreateActivity(11, ModuleId));
			var form = CreateForm(activityId: 11);

			// Act
			await _sut.UpdateAsync(1, form);

			// Assert: the tracked entity was changed and saved.
			Assert.Equal(11, assignment.ActivityId);
			Assert.Equal("Konsolapp", assignment.Name);
			_uow.Verify(u => u.CompleteAsync(), Times.Once);
		}

		/// <summary>
		/// Scenario: the teacher tries to move an assignment to an activity in another module.
		/// Expected: 400 (BadRequestException) and the assignment is unchanged.
		/// </summary>
		[Fact]
		public async Task UpdateAsync_ActivityInOtherModule_ThrowsBadRequest()
		{
			// Arrange: activity 20 belongs to module 4, the assignment to module 3.
			var assignment = new Assignment { Id = 1, ModuleId = ModuleId, ActivityId = ActivityId };
			_assignmentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(assignment);
			SetupActivity(CreateActivity(20, moduleId: 4));

			// Act & Assert
			await Assert.ThrowsAsync<BadRequestException>(
				() => _sut.UpdateAsync(1, CreateForm(activityId: 20)));
			Assert.Equal(ActivityId, assignment.ActivityId);
			_uow.Verify(u => u.CompleteAsync(), Times.Never);
		}

		/// <summary>
		/// Scenario: the teacher edits an assignment that does not exist.
		/// Expected: 404 (AssignmentNotFoundException).
		/// </summary>
		[Fact]
		public async Task UpdateAsync_AssignmentNotFound_Throws()
		{
			_assignmentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Assignment?)null);

			await Assert.ThrowsAsync<AssignmentNotFoundException>(
				() => _sut.UpdateAsync(1, CreateForm()));
		}

		/// <summary>
		/// Scenario: the teacher deletes an assignment that students have handed in.
		/// Expected: 409 (AssignmentHasSubmissionsException) and nothing is deleted.
		/// </summary>
		[Fact]
		public async Task DeleteAsync_WithSubmissions_ThrowsConflict()
		{
			var assignment = new Assignment { Id = 1 };
			_assignmentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(assignment);
			_assignmentRepository.Setup(r => r.HasSubmissionsAsync(1)).ReturnsAsync(true);

			await Assert.ThrowsAsync<AssignmentHasSubmissionsException>(() => _sut.DeleteAsync(1));
			_assignmentRepository.Verify(r => r.Delete(It.IsAny<Assignment>()), Times.Never);
			_uow.Verify(u => u.CompleteAsync(), Times.Never);
		}

		/// <summary>
		/// Scenario: the teacher deletes an assignment without submissions.
		/// Expected: the assignment is deleted and saved.
		/// </summary>
		[Fact]
		public async Task DeleteAsync_WithoutSubmissions_DeletesAssignment()
		{
			var assignment = new Assignment { Id = 1 };
			_assignmentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(assignment);
			_assignmentRepository.Setup(r => r.HasSubmissionsAsync(1)).ReturnsAsync(false);

			await _sut.DeleteAsync(1);

			_assignmentRepository.Verify(r => r.Delete(assignment), Times.Once);
			_uow.Verify(u => u.CompleteAsync(), Times.Once);
		}

		/// <summary>
		/// Scenario: the teacher lists assignments for a module that does not exist.
		/// Expected: 404 (ModuleNotFoundException) instead of an empty list.
		/// </summary>
		[Fact]
		public async Task GetForModuleAsync_ModuleNotFound_Throws()
		{
			_moduleRepository.Setup(r => r.GetByIdAsync(ModuleId)).ReturnsAsync((Module?)null);

			await Assert.ThrowsAsync<ModuleNotFoundException>(() => _sut.GetForModuleAsync(ModuleId));
		}

		// Makes the activity repository return the given activity for its id.
		private void SetupActivity(Activity activity)
		{
			_activityRepository
				.Setup(r => r.GetWithModuleAndCourseAsync(activity.Id))
				.ReturnsAsync(activity);
		}

		// An activity with its module and course loaded, like GetWithModuleAndCourseAsync returns it.
		private static Activity CreateActivity(int id, int moduleId)
		{
			var course = new Course { Id = 1, EndDate = CourseEnd };
			var module = new Module { Id = moduleId, EndDate = ModuleEnd, Course = course };
			return new Activity { Id = id, ModuleId = moduleId, Module = module };
		}

		// A valid form. The name has spaces to check that the service trims it.
		private static AssignmentFormDto CreateForm(int activityId = ActivityId, DateTime? deadline = null) => new()
		{
			ActivityId = activityId,
			Name = "  Konsolapp  ",
			Description = "Bygg en konsolapp.",
			Deadline = deadline,
			RequiresSubmission = true
		};
	}
}