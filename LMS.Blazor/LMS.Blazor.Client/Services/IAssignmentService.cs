using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Components.Forms;

namespace LMS.Blazor.Client.Services
{
	public interface IAssignmentService
	{		
		Task<IReadOnlyList<StudentAssignmentDto>?> GetMyAssignmentsAsync(CancellationToken cancellationToken = default);
				
		Task<(StudentAssignmentDto? Assignment, string? ErrorMessage)> SubmitAsync(
			int assignmentId, 
			IBrowserFile file, 
			CancellationToken cancellationToken = default);
	}
}
