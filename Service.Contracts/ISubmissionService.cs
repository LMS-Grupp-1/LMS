using LMS.Shared.DTOs.AssignmentDtos;

namespace Service.Contracts
{
	public interface ISubmissionService
	{		
		Task<StudentAssignmentDto> SubmitAsync(
			int assignmentId,
			string userId,
			string fileName,
			Stream content,
			CancellationToken cancellationToken);
	}
}
