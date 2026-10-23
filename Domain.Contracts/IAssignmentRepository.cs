using Domain.Models.Entities;
using LMS.Shared.DTOs;

namespace Domain.Contracts
{
	public interface IAssignmentRepository
	{		
		Task<IReadOnlyList<Assignment>> GetForStudentAsync(int courseId, string studentId);

        Task<IReadOnlyList<DashboardAssignmentDto>> GetActiveAssignmentsAsync();
    }
}
