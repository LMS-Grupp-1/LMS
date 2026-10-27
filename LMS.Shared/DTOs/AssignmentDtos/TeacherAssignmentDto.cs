namespace LMS.Shared.DTOs.AssignmentDtos
{	
	public record TeacherAssignmentDto(
		int Id,
		string Name,
		string Description,
		DateTime? Deadline,
		bool RequiresSubmission,
		int ActivityId,
		string ActivityName,		
		int SubmissionCount);
}
