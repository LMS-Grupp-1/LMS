using LMS.Shared.DTOs.FeedbackDtos;

namespace LMS.Shared.DTOs.AssignmentDtos
{	
	public record StudentAssignmentDto(
		int Id,
		string Name,
		string Description,
		DateTime? Deadline,
		int ModuleId,
		string ModuleName,
		bool RequiresSubmission,
		AssignmentStatus Status,
		DateTimeOffset? SubmittedAt,
		bool HasFeedback,
        IEnumerable<FeedbackForStudentDto> Feedbacks);
}
