using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions.AssignmentExceptions;
using Domain.Models.Exceptions.CourseExceptions;
using Domain.Models.Exceptions.DocumentExceptions;
using Domain.Models.Exceptions.SubmissionExceptions;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text;

namespace LMS.Services.Tests
{
	/// <summary>
	/// Unit tests for <see cref="SubmissionService"/>.
	/// </summary>
	/// <remarks>
	/// How these tests work:
	/// - The repositories, the unit of work and the file storage are mocks (Moq),
	///   so the tests run without a database and without Azurite.
	/// - The clock is fixed with FixedTimeProvider, so "now" is always the same time.
	/// - The storage mock lets us check WHEN a file is saved or deleted:
	///   no file may be saved if a check fails, and the file must be removed
	///   if saving the database rows fails.
	/// - The file rules themselves (allowed types, size) live in DocumentFiles
	///   and are the same as for course documents.
	///
	/// Not covered here:
	/// - The multipart upload and size limits in AssignmentsController,
	///   which are handled by ASP.NET Core and were tested manually in Swagger.
	/// - The database query in AssignmentRepository.GetByIdForStudentAsync.
	/// </remarks>
	public class SubmissionServiceTests
	{
		private const string StudentId = "student-1";
		private const int CourseId = 7;
		private const int AssignmentId = 1;

		// "Now" in every test: Wednesday 11 November 2026 12:00 UTC.
		private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

		private readonly Mock<ICourseRepository> _courseRepository = new();
		private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
		private readonly Mock<ISubmissionRepository> _submissionRepository = new();
		private readonly Mock<IUnitOfWork> _uow = new();
		private readonly Mock<IDocumentStorage> _storage = new();
		private readonly SubmissionService _sut;

		// The submission the service tried to add, captured so the tests can inspect it.
		private Submission? _addedSubmission;

		public SubmissionServiceTests()
		{
			_uow.Setup(u => u.Courses).Returns(_courseRepository.Object);
			_uow.Setup(u => u.Assignments).Returns(_assignmentRepository.Object);
			_uow.Setup(u => u.Submissions).Returns(_submissionRepository.Object);

			// By default the student belongs to course 7.
			_courseRepository
				.Setup(r => r.GetForStudentAsync(StudentId, false))
				.ReturnsAsync(new Course { Id = CourseId });

			// Capture the submission when the service adds it.
			_submissionRepository
				.Setup(r => r.AddAsync(It.IsAny<Submission>()))
				.Callback<Submission>(submission => _addedSubmission = submission)
				.Returns(Task.CompletedTask);

			// NullLogger: the service may log, but the tests do not care what.
			_sut = new SubmissionService(
				_uow.Object,
				_storage.Object,
				new FixedTimeProvider(Now),
				NullLogger<SubmissionService>.Instance);
		}

		/// <summary>
		/// Scenario: a student tries to hand in to an assignment in another course,
		/// for example by changing the id in the URL.
		/// Expected: AssignmentNotFoundException (404), and no file is saved.
		/// This protects the acceptance criterion "the student cannot hand in to an assignment in another course".
		/// </summary>
		[Fact]
		public async Task SubmitAsync_AssignmentInAnotherCourse_ThrowsAssignmentNotFound()
		{
			// Arrange: the assignment exists, but belongs to course 99.
			SetupAssignment(CreateAssignment(courseId: 99));

			// Act & Assert
			await Assert.ThrowsAsync<AssignmentNotFoundException>(
				() => _sut.SubmitAsync(AssignmentId, StudentId, "report.pdf", CreateFile(), CancellationToken.None));

			// Assert: nothing was saved, neither the file nor the database rows.
			VerifyNothingSaved();
		}

		/// <summary>
		/// Scenario: the assignment id does not exist.
		/// Expected: the same AssignmentNotFoundException as for another course,
		/// so a student cannot tell the two cases apart.
		/// </summary>
		[Fact]
		public async Task SubmitAsync_AssignmentDoesNotExist_ThrowsAssignmentNotFound()
		{
			// Arrange: the repository finds no assignment.
			_assignmentRepository
				.Setup(r => r.GetByIdForStudentAsync(AssignmentId, StudentId))
				.ReturnsAsync((Assignment?)null);

			// Act & Assert
			await Assert.ThrowsAsync<AssignmentNotFoundException>(
				() => _sut.SubmitAsync(AssignmentId, StudentId, "report.pdf", CreateFile(), CancellationToken.None));

			VerifyNothingSaved();
		}

		/// <summary>
		/// Scenario: a student tries to hand in to an assignment that does not require a submission,
		/// for example "read chapter 4".
		/// Expected: SubmissionValidationException (400), and no file is saved.
		/// </summary>
		[Fact]
		public async Task SubmitAsync_NoSubmissionRequired_ThrowsSubmissionValidation()
		{
			// Arrange: an assignment in the student's course that does not require a submission.
			SetupAssignment(CreateAssignment(requiresSubmission: false));

			// Act & Assert
			await Assert.ThrowsAsync<SubmissionValidationException>(
				() => _sut.SubmitAsync(AssignmentId, StudentId, "report.pdf", CreateFile(), CancellationToken.None));

			VerifyNothingSaved();
		}

		/// <summary>
		/// Scenario: a student tries to hand in a file type that is not allowed.
		/// Expected: DocumentValidationException (400) from the shared file rules, and no file is saved.
		/// This protects the acceptance criterion "file type and file size are validated".
		/// </summary>
		[Fact]
		public async Task SubmitAsync_FileTypeNotAllowed_ThrowsDocumentValidation()
		{
			// Arrange: a valid assignment, but the file is an .exe.
			SetupAssignment(CreateAssignment());

			// Act & Assert
			await Assert.ThrowsAsync<DocumentValidationException>(
				() => _sut.SubmitAsync(AssignmentId, StudentId, "virus.exe", CreateFile(), CancellationToken.None));

			VerifyNothingSaved();
		}

		/// <summary>
		/// Scenario: a student hands in a valid file before the deadline.
		/// Expected: the submission is linked to the right student and assignment,
		/// the time comes from the server clock, the file is saved under the document's blob path,
		/// and the returned status is Submitted.
		/// This protects the acceptance criteria "linked to the right student and assignment"
		/// and "the submission time is set by the system".
		/// </summary>
		[Fact]
		public async Task SubmitAsync_ValidFileBeforeDeadline_SavesSubmissionAndReturnsSubmitted()
		{
			// Arrange: deadline Friday 13 November 17:00 Swedish time, two days after "now".
			var assignment = CreateAssignment(deadline: new DateTime(2026, 11, 13, 17, 0, 0));
			SetupAssignment(assignment);
			AddSubmissionToAssignmentWhenSaved(assignment);

			// Act
			var result = await _sut.SubmitAsync(
				AssignmentId, StudentId, "report.pdf", CreateFile(), CancellationToken.None);

			// Assert: the submission that was added has the right links and the server time.
			Assert.NotNull(_addedSubmission);
			Assert.Equal(AssignmentId, _addedSubmission.AssignmentId);
			Assert.Equal(StudentId, _addedSubmission.StudentId);
			Assert.Equal(Now, _addedSubmission.SubmittedAt);

			// Assert: the document has the cleaned file name and the content type from the file extension.
			Assert.Equal("report.pdf", _addedSubmission.Document.FileName);
			Assert.Equal("application/pdf", _addedSubmission.Document.ContentType);

			// Assert: the file was saved once, under the same path as the document row.
			_storage.Verify(s => s.SaveAsync(
					_addedSubmission.Document.BlobPath,
					It.IsAny<Stream>(),
					"application/pdf",
					It.IsAny<CancellationToken>()),
				Times.Once);
			_uow.Verify(u => u.CompleteAsync(), Times.Once);

			// Assert: the returned assignment shows the new status.
			Assert.Equal(AssignmentStatus.Submitted, result.Status);
			Assert.Equal(Now, result.SubmittedAt);
		}

		/// <summary>
		/// Scenario: a student hands in a valid file after the deadline.
		/// Expected: the submission is accepted, but the status is Late.
		/// This protects the acceptance criterion "the system can tell whether the submission is late".
		/// </summary>
		[Fact]
		public async Task SubmitAsync_ValidFileAfterDeadline_ReturnsLate()
		{
			// Arrange: the deadline was Monday 9 November 17:00 Swedish time, two days before "now".
			var assignment = CreateAssignment(deadline: new DateTime(2026, 11, 9, 17, 0, 0));
			SetupAssignment(assignment);
			AddSubmissionToAssignmentWhenSaved(assignment);

			// Act
			var result = await _sut.SubmitAsync(
				AssignmentId, StudentId, "report.pdf", CreateFile(), CancellationToken.None);

			// Assert: a late submission is still saved, but marked as late.
			_uow.Verify(u => u.CompleteAsync(), Times.Once);
			Assert.Equal(AssignmentStatus.Late, result.Status);
		}

		/// <summary>
		/// Scenario: the file is saved in storage, but saving the database rows fails.
		/// Expected: the file is deleted again, so no orphaned files are left,
		/// and the original error is passed on.
		/// </summary>
		[Fact]
		public async Task SubmitAsync_DatabaseSaveFails_DeletesUploadedFile()
		{
			// Arrange: a valid assignment, but CompleteAsync throws (for example a lost database connection).
			SetupAssignment(CreateAssignment());
			_uow
				.Setup(u => u.CompleteAsync())
				.ThrowsAsync(new InvalidOperationException("Database is down"));

			// Act & Assert: the original exception reaches the caller.
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => _sut.SubmitAsync(AssignmentId, StudentId, "report.pdf", CreateFile(), CancellationToken.None));

			// Assert: the file that was saved is deleted again, using the same path.
			Assert.NotNull(_addedSubmission);
			_storage.Verify(s => s.DeleteAsync(
					_addedSubmission.Document.BlobPath,
					It.IsAny<CancellationToken>()),
				Times.Once);
		}

		/// <summary>
		/// Scenario: a teacher asks for a course id that does not exist.
		/// Expected: CourseNotFoundException (404) instead of an empty list.
		/// </summary>
		[Fact]
		public async Task GetForCourseAsync_CourseDoesNotExist_ThrowsCourseNotFound()
		{
			// Arrange: the course repository finds no course with id 99.
			_courseRepository
				.Setup(r => r.GetByIdAsync(99, false))
				.ReturnsAsync((Course?)null);

			// Act & Assert
			await Assert.ThrowsAsync<CourseNotFoundException>(
				() => _sut.GetForCourseAsync(99, assignmentId: null, status: null));
		}

		/// <summary>
		/// Scenario: two students and one assignment whose deadline has passed.
		/// Anna has handed in on time, Erik has not handed in at all.
		/// Expected: one row per student. Erik's row exists even though he has no submission,
		/// with status Late and no SubmissionId. This is how missing submissions become visible.
		/// </summary>
		[Fact]
		public async Task GetForCourseAsync_StudentWithoutSubmission_GetsRowWithoutSubmission()
		{
			// Arrange: deadline Monday 9 November 17:00 Swedish time (passed).
			// Anna handed in on Sunday 8 November, Erik never did.
			var assignment = CreateAssignment(deadline: new DateTime(2026, 11, 9, 17, 0, 0));
			assignment.Submissions.Add(CreateSubmission(10, "anna", new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero)));

			SetupTeacherOverview([assignment], [CreateStudent("anna", "Anna"), CreateStudent("erik", "Erik")]);

			// Act
			var rows = await _sut.GetForCourseAsync(CourseId, assignmentId: null, status: null);

			// Assert: two rows, one per student.
			Assert.Equal(2, rows.Count);

			var anna = Assert.Single(rows, row => row.StudentId == "anna");
			Assert.Equal(AssignmentStatus.Submitted, anna.Status);
			Assert.Equal(10, anna.SubmissionId);
			Assert.Equal("report.pdf", anna.FileName);

			// Erik's row has no submission, and the deadline has passed, so he is late.
			var erik = Assert.Single(rows, row => row.StudentId == "erik");
			Assert.Equal(AssignmentStatus.Late, erik.Status);
			Assert.Null(erik.SubmissionId);
			Assert.Null(erik.SubmittedAt);
		}

		/// <summary>
		/// Scenario: a student has handed in twice; only the second time has feedback.
		/// Expected: the row uses the latest submission, so the id, time and feedback come from it.
		/// </summary>
		[Fact]
		public async Task GetForCourseAsync_SeveralSubmissions_UsesTheLatest()
		{
			// Arrange: deadline Friday 13 November (not passed). Anna handed in on the 9th and the 10th.
			var assignment = CreateAssignment(deadline: new DateTime(2026, 11, 13, 17, 0, 0));
			assignment.Submissions.Add(CreateSubmission(10, "anna", new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero)));
			assignment.Submissions.Add(CreateSubmission(11, "anna", new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero), withFeedback: true));

			SetupTeacherOverview([assignment], [CreateStudent("anna", "Anna")]);

			// Act
			var row = Assert.Single(await _sut.GetForCourseAsync(CourseId, assignmentId: null, status: null));

			// Assert: submission 11 is the latest, and it has feedback.
			Assert.Equal(11, row.SubmissionId);
			Assert.True(row.HasFeedback);
		}

		/// <summary>
		/// Scenario: the teacher filters on status Late.
		/// Expected: only rows with that status are returned.
		/// </summary>
		[Fact]
		public async Task GetForCourseAsync_StatusFilter_ReturnsOnlyMatchingRows()
		{
			// Arrange: the same situation as above, Anna handed in and Erik is late.
			var assignment = CreateAssignment(deadline: new DateTime(2026, 11, 9, 17, 0, 0));
			assignment.Submissions.Add(CreateSubmission(10, "anna", new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero)));

			SetupTeacherOverview([assignment], [CreateStudent("anna", "Anna"), CreateStudent("erik", "Erik")]);

			// Act
			var rows = await _sut.GetForCourseAsync(CourseId, assignmentId: null, status: AssignmentStatus.Late);

			// Assert: only Erik is left.
			var row = Assert.Single(rows);
			Assert.Equal("erik", row.StudentId);
		}

		/// <summary>
		/// Scenario: the course has two assignments and the teacher filters on one of them.
		/// Expected: only rows for that assignment are returned.
		/// </summary>
		[Fact]
		public async Task GetForCourseAsync_AssignmentFilter_ReturnsOnlyThatAssignment()
		{
			// Arrange: two assignments with ids 1 and 2.
			var first = CreateAssignment();
			var second = CreateAssignment();
			second.Id = 2;

			SetupTeacherOverview([first, second], [CreateStudent("anna", "Anna")]);

			// Act
			var rows = await _sut.GetForCourseAsync(CourseId, assignmentId: 2, status: null);

			// Assert: one row, for assignment 2.
			var row = Assert.Single(rows);
			Assert.Equal(2, row.AssignmentId);
		}
		
		/// <summary>
		/// Scenario: a teacher downloads a student's submission.
		/// Expected: the file is read from storage, with the stored content type and file name.
		/// </summary>
		[Fact]
		public async Task DownloadAsync_Teacher_ReturnsTheFile()
		{
			// Arrange: submission 10 belongs to Anna; the caller is a teacher.
			SetupDownload(CreateSubmission(10, "anna", Now));

			// Act
			var download = await _sut.DownloadAsync(10, "teacher-1", isTeacher: true, CancellationToken.None);

			// Assert
			Assert.Equal("report.pdf", download.FileName);
			Assert.Equal("application/pdf", download.ContentType);
			_storage.Verify(s => s.OpenReadAsync("blob-10", It.IsAny<CancellationToken>()), Times.Once);
		}

		/// <summary>
		/// Scenario: a student downloads their own submission.
		/// Expected: allowed.
		/// </summary>
		[Fact]
		public async Task DownloadAsync_StudentOwnSubmission_ReturnsTheFile()
		{
			// Arrange: submission 10 belongs to Anna, and Anna is asking.
			SetupDownload(CreateSubmission(10, "anna", Now));

			// Act
			var download = await _sut.DownloadAsync(10, "anna", isTeacher: false, CancellationToken.None);

			// Assert
			Assert.Equal("report.pdf", download.FileName);
		}

		/// <summary>
		/// Scenario: a student tries to download another student's submission, for example by guessing an id.
		/// Expected: the same SubmissionNotFoundException (404) as for an id that does not exist,
		/// and the file is never read from storage.
		/// </summary>
		[Fact]
		public async Task DownloadAsync_StudentOtherSubmission_ThrowsSubmissionNotFound()
		{
			// Arrange: submission 10 belongs to Anna, but Erik is asking.
			SetupDownload(CreateSubmission(10, "anna", Now));

			// Act & Assert
			await Assert.ThrowsAsync<SubmissionNotFoundException>(
				() => _sut.DownloadAsync(10, "erik", isTeacher: false, CancellationToken.None));

			_storage.Verify(s => s.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		/// <summary>
		/// Scenario: the submission id does not exist.
		/// Expected: SubmissionNotFoundException (404), even for a teacher.
		/// </summary>
		[Fact]
		public async Task DownloadAsync_SubmissionDoesNotExist_ThrowsSubmissionNotFound()
		{
			// Arrange: the repository finds nothing.
			_submissionRepository
				.Setup(r => r.GetWithDocumentAsync(99))
				.ReturnsAsync((Submission?)null);

			// Act & Assert
			await Assert.ThrowsAsync<SubmissionNotFoundException>(
				() => _sut.DownloadAsync(99, "teacher-1", isTeacher: true, CancellationToken.None));
		}

		// --- Helpers ---------------------------------------------------------

		// An assignment in the student's course (course 7) unless another course is given.
		private static Assignment CreateAssignment(
			int courseId = CourseId,
			bool requiresSubmission = true,
			DateTime? deadline = null) => new()
			{
				Id = AssignmentId,
				Name = "Report",
				Deadline = deadline,
				RequiresSubmission = requiresSubmission,
				ModuleId = 3,
				Module = new Module { Id = 3, CourseId = courseId, Name = "C#" }
			};

		// The repository returns the given assignment, both before and after saving.
		private void SetupAssignment(Assignment assignment) =>
			_assignmentRepository
				.Setup(r => r.GetByIdForStudentAsync(AssignmentId, StudentId))
				.ReturnsAsync(assignment);

		// A real database would return the new submission when the assignment is read again.
		// Here we add it to the assignment's list ourselves when CompleteAsync is called.
		private void AddSubmissionToAssignmentWhenSaved(Assignment assignment) =>
			_uow
				.Setup(u => u.CompleteAsync())
				.Callback(() => assignment.Submissions.Add(_addedSubmission!))
				.ReturnsAsync(1);

		// A small file with some text, the content does not matter.
		private static MemoryStream CreateFile() => new(Encoding.UTF8.GetBytes("Test content"));

		// A student with only the fields the overview uses.
		private static ApplicationUser CreateStudent(string id, string firstName) => new()
		{
			Id = id,
			FirstName = firstName,
			LastName = "Test"
		};

		// A submission with a PDF document. The blob path includes the id, so tests can check it.
		private static Submission CreateSubmission(
			int id,
			string studentId,
			DateTimeOffset submittedAt,
			bool withFeedback = false) => new()
			{
				Id = id,
				AssignmentId = AssignmentId,
				StudentId = studentId,
				SubmittedAt = submittedAt,
				Document = new Document
				{
					FileName = "report.pdf",
					ContentType = "application/pdf",
					BlobPath = $"blob-{id}"
				},
				Feedbacks = withFeedback ? new List<Feedback>
					{
						new Feedback { Comment = "Good" }
					} : new List<Feedback>()
            };

		// The course exists, and has the given assignments and students.
		private void SetupTeacherOverview(List<Assignment> assignments, List<ApplicationUser> students)
		{
			_courseRepository
				.Setup(r => r.GetByIdAsync(CourseId, false))
				.ReturnsAsync(new Course { Id = CourseId });
			_assignmentRepository
				.Setup(r => r.GetWithSubmissionsForCourseAsync(CourseId))
				.ReturnsAsync(assignments);
			_courseRepository
				.Setup(r => r.GetStudentsAsync(CourseId))
				.ReturnsAsync(students);
		}

		// The repository finds the submission, and storage returns an empty stream for its file.
		private void SetupDownload(Submission submission)
		{
			_submissionRepository
				.Setup(r => r.GetWithDocumentAsync(submission.Id))
				.ReturnsAsync(submission);
			_storage
				.Setup(s => s.OpenReadAsync(submission.Document.BlobPath, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream());
		}

		// No file in storage and no database save.
		private void VerifyNothingSaved()
		{
			_storage.Verify(s => s.SaveAsync(
					It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
				Times.Never);
			_submissionRepository.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Never);
			_uow.Verify(u => u.CompleteAsync(), Times.Never);
		}

		// A clock that always returns the same time.
		private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
		{
			public override DateTimeOffset GetUtcNow() => now;
		}
	}
}