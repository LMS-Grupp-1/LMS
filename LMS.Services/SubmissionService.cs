using Domain.Contracts;
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

		private static string Truncate(string value, int maxLength) =>
			value.Length <= maxLength ? value : value[..maxLength];
	}
}
