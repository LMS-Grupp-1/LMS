using LMS.Shared.DTOs.AssignmentDtos;

namespace Service.Contracts
{
	public interface IAssignmentService
	{
		Task<IReadOnlyList<StudentAssignmentDto>> GetMyAssignmentsAsync(string userId);
	}
}
