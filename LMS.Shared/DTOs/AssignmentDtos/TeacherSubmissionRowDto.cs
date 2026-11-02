namespace LMS.Shared.DTOs.AssignmentDtos
{	
	public record TeacherSubmissionRowDto(
		int AssignmentId,
		string AssignmentName,
		string ModuleName,
		DateTime? Deadline,
		string StudentId,
		string StudentName,
		AssignmentStatus Status,
		int? SubmissionId,
		DateTimeOffset? SubmittedAt,
		string? FileName,
		bool HasFeedback,
		int FeedbackCount);
}
