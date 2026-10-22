using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.AssignmentDtos;
using Moq;

namespace LMS.Services.Tests
{
	/// <summary>
	/// Unit tests for <see cref="AssignmentService"/>.
	/// </summary>
	/// <remarks>
	/// How these tests work:
	/// - The service gets its data from repositories through the unit of work.
	///   Here the repositories are replaced with mocks (fake objects from Moq),
	///   so the tests run without a database.
	/// - With .Setup(...).ReturnsAsync(...) we decide what a mock returns
	///   when the service calls it, for example "this student belongs to course 7".
	/// - With .Verify(...) we check afterwards that the service called a mock
	///   with the expected arguments.
	/// - The clock is replaced with FixedTimeProvider (at the bottom of the file),
	///   so "now" is always the same time and the tests give the same result every day.
	/// - The status rules themselves are tested in AssignmentStatusCalculatorTests.
	///   These tests check that the service fetches the right data and passes it on correctly.
	///
	/// Not covered here:
	/// - The database query in AssignmentRepository (Include, Where, OrderBy),
	///   which requires an integration test against a real database.
	/// - Role-based access on AssignmentsController (Student only),
	///   which is handled by ASP.NET Core and was tested manually in Swagger.
	/// </remarks>
	public class AssignmentServiceTests
	{
		// The id of the logged-in student in every test.
		private const string StudentId = "student-1";

		// "Now" in every test: Wednesday 11 November 2026 12:00 UTC.
		// It is two days before the deadline used in the tests below.
		private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

		// The mocks. Each one replaces a real repository.
		private readonly Mock<ICourseRepository> _courseRepository = new();
		private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
		private readonly Mock<IUnitOfWork> _uow = new();

		// "System under test": the real service, which uses the mocks above.
		private readonly AssignmentService _sut;

		// xUnit creates a new instance of this class for every test,
		// so each test starts with fresh mocks and no shared state.
		public AssignmentServiceTests()
		{
			// The service reaches the repositories through the unit of work,
			// so the unit of work mock returns the repository mocks.
			_uow.Setup(u => u.Courses).Returns(_courseRepository.Object);
			_uow.Setup(u => u.Assignments).Returns(_assignmentRepository.Object);

			_sut = new AssignmentService(_uow.Object, new FixedTimeProvider(Now));
		}

		/// <summary>
		/// Scenario: a student who is not enrolled in any course asks for their assignments.
		/// Expected: <see cref="StudentCourseNotFoundException"/>, which the global exception
		/// handler turns into a 404 with a clear message instead of an empty list.
		/// </summary>
		[Fact]
		public async Task GetMyAssignmentsAsync_StudentWithoutCourse_ThrowsStudentCourseNotFoundException()
		{
			// Arrange: the course repository finds no course for the student (returns null).
			_courseRepository
				.Setup(r => r.GetForStudentAsync(StudentId, false))
				.ReturnsAsync((Course?)null);

			// Act & Assert: calling the service must throw the exception.
			await Assert.ThrowsAsync<StudentCourseNotFoundException>(
				() => _sut.GetMyAssignmentsAsync(StudentId));
		}

		/// <summary>
		/// Scenario: a student enrolled in course 7 asks for their assignments.
		/// Expected: the service asks for the assignments in course 7, for this student.
		/// This protects the acceptance criterion "the student only sees assignments in their own course":
		/// the course always comes from the logged-in student, never from the request.
		/// </summary>
		[Fact]
		public async Task GetMyAssignmentsAsync_UsesTheStudentsOwnCourse()
		{
			// Arrange: the student belongs to course 7, and the course has no assignments.
			_courseRepository
				.Setup(r => r.GetForStudentAsync(StudentId, false))
				.ReturnsAsync(new Course { Id = 7 });
			_assignmentRepository
				.Setup(r => r.GetForStudentAsync(7, StudentId))
				.ReturnsAsync([]);

			// Act
			await _sut.GetMyAssignmentsAsync(StudentId);

			// Assert: the assignment repository was called exactly once,
			// with course 7 and the logged-in student's id.
			_assignmentRepository.Verify(r => r.GetForStudentAsync(7, StudentId), Times.Once);
		}

		/// <summary>
		/// Scenario: the student has handed in twice, and the latest submission has feedback.
		/// Expected: the latest submission decides the status and SubmittedAt,
		/// HasFeedback is true, and the module name is copied to the DTO.
		/// </summary>
		[Fact]
		public async Task GetMyAssignmentsAsync_SeveralSubmissions_UsesTheLatest()
		{
			// Arrange: an assignment with the deadline Friday 13 November 17:00 Swedish time (16:00 UTC).
			// The student handed in on Tuesday 10 November and again on Wednesday 11 November.
			// Both are before the deadline, and only the second one has feedback.
			var firstSubmission = new Submission
			{
				SubmittedAt = new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero)
			};
			var latestSubmission = new Submission
			{
				SubmittedAt = new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero),
				Feedback = new Feedback { Comment = "Good" }
			};

			var assignment = new Assignment
			{
				Id = 1,
				Name = "Report",
				Deadline = new DateTime(2026, 11, 13, 17, 0, 0), // Swedish local time, as stored in the database
				RequiresSubmission = true,
				Module = new Module { Name = "C#" },
				Submissions = [firstSubmission, latestSubmission]
			};

			// The student belongs to course 7, and course 7 contains the assignment above.
			_courseRepository
				.Setup(r => r.GetForStudentAsync(StudentId, false))
				.ReturnsAsync(new Course { Id = 7 });
			_assignmentRepository
				.Setup(r => r.GetForStudentAsync(7, StudentId))
				.ReturnsAsync([assignment]);

			// Act: Assert.Single also checks that exactly one assignment is returned.
			var result = Assert.Single(await _sut.GetMyAssignmentsAsync(StudentId));

			// Assert
			// Handed in before the deadline, so the status is Submitted.
			Assert.Equal(AssignmentStatus.Submitted, result.Status);
			// SubmittedAt comes from the latest submission, not the first.
			Assert.Equal(latestSubmission.SubmittedAt, result.SubmittedAt);
			// The latest submission has feedback.
			Assert.True(result.HasFeedback);
			// The module name comes from the related module.
			Assert.Equal("C#", result.ModuleName);
		}

		/// <summary>
		/// A clock that always returns the same time.
		/// The service gets "now" from TimeProvider.GetUtcNow(). In production that is the real clock
		/// (TimeProvider.System); in the tests it is this class, so the result never depends on today's date.
		/// </summary>
		private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
		{
			public override DateTimeOffset GetUtcNow() => now;
		}
	}
}