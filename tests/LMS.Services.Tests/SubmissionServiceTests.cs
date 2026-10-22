using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
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