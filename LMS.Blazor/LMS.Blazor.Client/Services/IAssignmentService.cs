using LMS.Shared.DTOs.AssignmentDtos;

namespace LMS.Blazor.Client.Services
{
	public interface IAssignmentService
	{		
		Task<IReadOnlyList<StudentAssignmentDto>?> GetMyAssignmentsAsync(CancellationToken cancellationToken = default);
	}
}
