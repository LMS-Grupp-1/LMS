using Domain.Contracts;
using Domain.Models.Documents;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.Extensions.Logging;
using Service.Contracts;

namespace LMS.Services
{
	public sealed class SubmissionService(
	IUnitOfWork uow,
	IDocumentStorage storage,
	TimeProvider timeProvider,
	ILogger<SubmissionService> logger) : ISubmissionService
	{
		public async Task<StudentAssignmentDto> SubmitAsync(
			int assignmentId,
			string userId,
			string fileName,
			Stream content,
			CancellationToken cancellationToken)
		{
			var course = await uow.Courses.GetForStudentAsync(userId, trackChanges: false)
				?? throw new StudentCourseNotFoundException();

			var assignment = await uow.Assignments.GetByIdForStudentAsync(assignmentId, userId);
			if (assignment is null || assignment.Module.CourseId != course.Id)
			{
				throw new AssignmentNotFoundException();
			}

			if (!assignment.RequiresSubmission)
			{
				throw new SubmissionValidationException(
					"This assignment does not require a submission.");
			}

			var (cleanFileName, contentType) = DocumentFiles.ValidateFileName(fileName);
			using var bufferedContent = await DocumentFiles.ReadAsync(content, cancellationToken);

			var submittedAt = timeProvider.GetUtcNow();

			var submission = new Submission
			{
				AssignmentId = assignment.Id,
				StudentId = userId,
				SubmittedAt = submittedAt,
				Document = new Document
				{
					Name = Truncate($"Inlämning – {assignment.Name}", DocumentConstraints.MaxNameLength),
					Description = string.Empty,
					FileName = cleanFileName,
					ContentType = contentType,
					SizeBytes = bufferedContent.Length,
					BlobPath = Guid.NewGuid().ToString("N"),
					UploadedAt = submittedAt,
					UploadedById = userId
				}
			};

			await storage.SaveAsync(
				submission.Document.BlobPath, bufferedContent, contentType, cancellationToken);

			try
			{
				await uow.Submissions.AddAsync(submission);
				await uow.CompleteAsync();
			}
			catch
			{
				try
				{
					await storage.DeleteAsync(submission.Document.BlobPath, CancellationToken.None);
				}
				catch (Exception cleanupException)
				{
					logger.LogError(
						cleanupException,
						"Could not remove blob {BlobPath} after a failed submission.",
						submission.Document.BlobPath);
				}

				throw;
			}

			var updated = await uow.Assignments.GetByIdForStudentAsync(assignmentId, userId)
				?? throw new AssignmentNotFoundException();

			return StudentAssignmentMapper.ToDto(updated, timeProvider.GetUtcNow());
		}

		public async Task<IReadOnlyList<TeacherSubmissionRowDto>> GetForCourseAsync(
			int courseId,
			int? assignmentId,
			AssignmentStatus? status)
		{			
			_ = await uow.Courses.GetByIdAsync(courseId, trackChanges: false)
				?? throw new CourseNotFoundException(courseId);

			var assignments = await uow.Assignments.GetWithSubmissionsForCourseAsync(courseId);
			var students = await uow.Courses.GetStudentsAsync(courseId);
			var now = timeProvider.GetUtcNow();

			var rows = new List<TeacherSubmissionRowDto>();

			foreach (var assignment in assignments)
			{
				if (assignmentId is not null && assignment.Id != assignmentId)
					continue;

				DateTimeOffset? deadline = assignment.Deadline is { } localDeadline
					? AssignmentStatusCalculator.ToSwedishTime(localDeadline)
					: null;

				// One row per student, also for students who have not handed in.
				foreach (var student in students)
				{
					// If the student has handed in more than once, the latest submission counts.
					var latest = assignment.Submissions
						.Where(submission => submission.StudentId == student.Id)
						.OrderByDescending(submission => submission.SubmittedAt)
						.FirstOrDefault();

					var rowStatus = AssignmentStatusCalculator.Calculate(
						requiresSubmission: true, deadline, latest?.SubmittedAt, now);

					if (status is not null && rowStatus != status)
						continue;

                    var feedbackCount = latest?.Feedbacks?.Count ?? 0;
                    rows.Add(new TeacherSubmissionRowDto(
						assignment.Id,
						assignment.Name,
						assignment.Module.Name,
						assignment.Deadline,
						student.Id,
						$"{student.FirstName} {student.LastName}",
						rowStatus,
						latest?.Id,
						latest?.SubmittedAt,
						latest?.Document.FileName,
                        feedbackCount > 0,
                        feedbackCount));
				}
			}

			return rows;
		}

		public async Task<DocumentDownload> DownloadAsync(
			int submissionId,
			string userId,
			bool isTeacher,
			CancellationToken cancellationToken)
		{
			var submission = await uow.Submissions.GetWithDocumentAsync(submissionId);
						
			if (submission is null || (!isTeacher && submission.StudentId != userId))
			{
				throw new SubmissionNotFoundException();
			}

			var stream = await storage.OpenReadAsync(submission.Document.BlobPath, cancellationToken);

			return new DocumentDownload(
				stream,
				submission.Document.ContentType,
				submission.Document.FileName);
		}

		private static string Truncate(string value, int maxLength) =>
			value.Length <= maxLength ? value : value[..maxLength];
	}
}
