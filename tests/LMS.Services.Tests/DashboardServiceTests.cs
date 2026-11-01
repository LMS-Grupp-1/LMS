using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions.CourseExceptions;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.DashboardDtos;
using LMS.Shared.DTOs.DocumentDtos;
using Moq;

namespace LMS.Services.Tests
{
	/// <summary>
	/// Unit tests for <see cref="DashboardService"/>.
	/// </summary>
	/// <remarks>
	/// How these tests work:
	/// - The dashboard has no data of its own; it puts together data from five repositories.
	///   All of them are mocks, and by default they return empty lists.
	/// - Each test sets up only the data it is about, and checks the matching section.
	/// - The clock is fixed: "now" is Wednesday 11 November 2026 12:00 UTC,
	///   which is 13:00 Swedish time (winter time, UTC+1).
	/// - The status of each assignment is calculated by AssignmentStatusCalculator,
	///   which has its own tests. These tests check that the dashboard puts
	///   each assignment in the right section.
	/// </remarks>
	public class DashboardServiceTests
	{
		private const string StudentId = "student-1";
		private const int CourseId = 7;

		private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);
		private static readonly DateTime NowSwedish = new(2026, 11, 11, 13, 0, 0);

		private readonly Mock<ICourseRepository> _courseRepository = new();
		private readonly Mock<IModuleRepository> _moduleRepository = new();
		private readonly Mock<IActivityRepository> _activityRepository = new();
		private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
		private readonly Mock<IDocumentRepository> _documentRepository = new();
		private readonly Mock<IUnitOfWork> _uow = new();
		private readonly DashboardService _sut;

		public DashboardServiceTests()
		{
			_uow.Setup(u => u.Courses).Returns(_courseRepository.Object);
			_uow.Setup(u => u.Modules).Returns(_moduleRepository.Object);
			_uow.Setup(u => u.Activities).Returns(_activityRepository.Object);
			_uow.Setup(u => u.Assignments).Returns(_assignmentRepository.Object);
			_uow.Setup(u => u.Documents).Returns(_documentRepository.Object);

			// By default the student belongs to course 7, and every repository returns an empty list.
			_courseRepository
				.Setup(r => r.GetForStudentAsync(StudentId, false))
				.ReturnsAsync(new Course { Id = CourseId, Name = "Lexicon LTU" });
			_moduleRepository
				.Setup(r => r.GetModulesByCourseIdAsync(CourseId))
				.ReturnsAsync([]);
			_activityRepository
				.Setup(r => r.GetUpcomingByCourseIdAsync(CourseId, It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync([]);
			_assignmentRepository
				.Setup(r => r.GetForStudentAsync(CourseId, StudentId))
				.ReturnsAsync([]);
			_documentRepository
				.Setup(r => r.ListAsync(CourseId, It.IsAny<CancellationToken>()))
				.ReturnsAsync([]);

			_sut = new DashboardService(_uow.Object, new FixedTimeProvider(Now));
		}

		/// <summary>
		/// Scenario: a student who is not enrolled in any course opens the dashboard.
		/// Expected: StudentCourseNotFoundException (404), which the page shows as
		/// "You are not enrolled in a course yet."
		/// </summary>
		[Fact]
		public async Task GetStudentDashboardAsync_StudentWithoutCourse_ThrowsStudentCourseNotFound()
		{
			// Arrange: no course is found for the student.
			_courseRepository
				.Setup(r => r.GetForStudentAsync(StudentId, false))
				.ReturnsAsync((Course?)null);

			// Act & Assert
			await Assert.ThrowsAsync<StudentCourseNotFoundException>(
				() => _sut.GetStudentDashboardAsync(StudentId, CancellationToken.None));
		}

		/// <summary>
		/// Scenario: the course has five assignments in different situations.
		/// Expected: each one ends up in the right section:
		/// - overdue: only the late one WITHOUT a submission
		/// - upcoming deadlines: only those with a deadline ahead, ordered as the repository returns them
		/// - feedback: only the one with feedback
		/// </summary>
		[Fact]
		public async Task GetStudentDashboardAsync_SplitsAssignmentsIntoSections()
		{
			// Arrange: deadlines relative to Wednesday 11 November.
			var missing = CreateAssignment(1, "Missing", deadline: new DateTime(2026, 11, 9, 17, 0, 0));
			var handedInLate = CreateAssignment(2, "Handed in late", deadline: new DateTime(2026, 11, 9, 17, 0, 0),
				submittedAt: new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero));
			var withFeedback = CreateAssignment(3, "With feedback", deadline: new DateTime(2026, 11, 13, 17, 0, 0),
				submittedAt: new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero), withFeedback: true);
			var upcoming = CreateAssignment(4, "Upcoming", deadline: new DateTime(2026, 11, 20, 17, 0, 0));
			var noDeadline = CreateAssignment(5, "No deadline", deadline: null);

			// The repository orders by deadline, assignments without a deadline last.
			_assignmentRepository
				.Setup(r => r.GetForStudentAsync(CourseId, StudentId))
				.ReturnsAsync([missing, handedInLate, withFeedback, upcoming, noDeadline]);

			// Act
			var result = await _sut.GetStudentDashboardAsync(StudentId, CancellationToken.None);

			// Assert: only the late assignment WITHOUT a submission is overdue.
			// "Handed in late" is late too, but there is nothing left for the student to do.
			var overdue = Assert.Single(result.OverdueAssignments);
			Assert.Equal("Missing", overdue.Name);
			Assert.Equal(AssignmentStatus.Late, overdue.Status);

			// Assert: deadlines ahead of "now", in the repository's order. No past deadlines, no null deadline.
			Assert.Equal(["With feedback", "Upcoming"], result.UpcomingDeadlines.Select(a => a.Name));

			// Assert: only the assignment with feedback.
			var feedback = Assert.Single(result.AssignmentsWithFeedback);
			Assert.Equal("With feedback", feedback.Name);
		}

		/// <summary>
		/// Scenario: the course has more upcoming deadlines than the dashboard shows.
		/// Expected: only the first five are returned.
		/// </summary>
		[Fact]
		public async Task GetStudentDashboardAsync_ManyDeadlines_ReturnsAtMostFive()
		{
			// Arrange: seven assignments with deadlines on 12–18 November.
			var assignments = Enumerable.Range(1, 7)
				.Select(day => CreateAssignment(day, $"Assignment {day}", deadline: new DateTime(2026, 11, 11 + day, 17, 0, 0)))
				.ToList();
			_assignmentRepository
				.Setup(r => r.GetForStudentAsync(CourseId, StudentId))
				.ReturnsAsync(assignments);

			// Act
			var result = await _sut.GetStudentDashboardAsync(StudentId, CancellationToken.None);

			// Assert: the five nearest.
			Assert.Equal(5, result.UpcomingDeadlines.Count);
			Assert.Equal("Assignment 1", result.UpcomingDeadlines[0].Name);
		}

		/// <summary>
		/// Scenario: today (11 November) is inside module "B" but not inside "A" or "C".
		/// Expected: module "B" is the current module.
		/// </summary>
		[Fact]
		public async Task GetStudentDashboardAsync_TodayInsideModule_ReturnsCurrentModule()
		{
			// Arrange
			_moduleRepository
				.Setup(r => r.GetModulesByCourseIdAsync(CourseId))
				.ReturnsAsync([
					CreateModule(1, "A", new DateTime(2026, 10, 1), new DateTime(2026, 10, 31)),
					CreateModule(2, "B", new DateTime(2026, 11, 1), new DateTime(2026, 11, 30)),
					CreateModule(3, "C", new DateTime(2026, 12, 1), new DateTime(2026, 12, 18))
				]);

			// Act
			var result = await _sut.GetStudentDashboardAsync(StudentId, CancellationToken.None);

			// Assert
			Assert.NotNull(result.CurrentModule);
			Assert.Equal("B", result.CurrentModule.Name);
		}

		/// <summary>
		/// Scenario: the module ends today. Module dates have no time of day,
		/// so the module must still count as current for the whole day.
		/// </summary>
		[Fact]
		public async Task GetStudentDashboardAsync_ModuleEndsToday_IsStillCurrent()
		{
			// Arrange: the module ends on 11 November at 00:00, but "now" is 13:00 the same day.
			_moduleRepository
				.Setup(r => r.GetModulesByCourseIdAsync(CourseId))
				.ReturnsAsync([CreateModule(1, "Ends today", new DateTime(2026, 11, 1), new DateTime(2026, 11, 11))]);

			// Act
			var result = await _sut.GetStudentDashboardAsync(StudentId, CancellationToken.None);

			// Assert
			Assert.Equal("Ends today", result.CurrentModule?.Name);
		}

		/// <summary>
		/// Scenario: today falls between two modules.
		/// Expected: no current module (null), which the page shows as "No module is running right now."
		/// </summary>
		[Fact]
		public async Task GetStudentDashboardAsync_BetweenModules_ReturnsNoCurrentModule()
		{
			// Arrange: one module ended on 5 November, the next starts on 16 November.
			_moduleRepository
				.Setup(r => r.GetModulesByCourseIdAsync(CourseId))
				.ReturnsAsync([
					CreateModule(1, "A", new DateTime(2026, 10, 1), new DateTime(2026, 11, 5)),
					CreateModule(2, "B", new DateTime(2026, 11, 16), new DateTime(2026, 12, 18))
				]);

			// Act
			var result = await _sut.GetStudentDashboardAsync(StudentId, CancellationToken.None);

			// Assert
			Assert.Null(result.CurrentModule);
		}

		/// <summary>
		/// Scenario: the dashboard asks for upcoming activities.
		/// Expected: "now" is passed as Swedish local time (13:00, not 12:00 UTC),
		/// since activity times are stored in Swedish time, and at most five are asked for.
		/// </summary>
		[Fact]
		public async Task GetStudentDashboardAsync_AsksForActivitiesInSwedishTime()
		{
			// Act
			await _sut.GetStudentDashboardAsync(StudentId, CancellationToken.None);

			// Assert: 13:00 Swedish time, and five activities.
			_activityRepository.Verify(r => r.GetUpcomingByCourseIdAsync(
					CourseId, NowSwedish, 5, It.IsAny<CancellationToken>()),
				Times.Once);
		}

		/// <summary>
		/// Scenario: the course has more documents than the dashboard shows.
		/// Expected: only the first five, which are the newest since the repository orders newest first.
		/// </summary>
		[Fact]
		public async Task GetStudentDashboardAsync_ManyDocuments_ReturnsTheFiveNewest()
		{
			// Arrange: eight documents, newest first.
			var documents = Enumerable.Range(1, 8).Select(CreateDocument).ToList();
			_documentRepository
				.Setup(r => r.ListAsync(CourseId, It.IsAny<CancellationToken>()))
				.ReturnsAsync(documents);

			// Act
			var result = await _sut.GetStudentDashboardAsync(StudentId, CancellationToken.None);

			// Assert
			Assert.Equal(5, result.RecentDocuments.Count);
			Assert.Equal(1, result.RecentDocuments[0].Id);
		}

		// --- Helpers ---------------------------------------------------------

		// An assignment that requires a submission, with an optional submission from the student.
		private static Assignment CreateAssignment(
			int id,
			string name,
			DateTime? deadline,
			DateTimeOffset? submittedAt = null,
			bool withFeedback = false)
		{
			var assignment = new Assignment
			{
				Id = id,
				Name = name,
				Deadline = deadline,
				RequiresSubmission = true,
				Module = new Module { Name = "C#" }
			};

			if (submittedAt is not null)
			{
				assignment.Submissions.Add(new Submission
				{
					SubmittedAt = submittedAt.Value,
					Feedbacks = withFeedback ? new List<Feedback>
						{
							new Feedback { Comment = "Good" }
						} : new List<Feedback>()
                });
			}

			return assignment;
		}

		private static Module CreateModule(int id, string name, DateTime start, DateTime end) => new()
		{
			Id = id,
			CourseId = CourseId,
			Name = name,
			StartDate = start,
			EndDate = end
		};

		// Only the id matters in these tests; the rest is filled in so the record can be created.
		private static DocumentDto CreateDocument(int id) => new(
			id, $"Document {id}", "", $"doc{id}.pdf", "application/pdf", 100,
			Now.AddDays(-id), "Teacher", CourseId, null, null, "Kursmaterial");

		// A clock that always returns the same time.
		private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
		{
			public override DateTimeOffset GetUtcNow() => now;
		}
	}
}