using Domain.Models.Entities;
using LMS.Shared.DTOs;

namespace Domain.Contracts
{
	public interface IAssignmentRepository
	{		
		Task<IReadOnlyList<Assignment>> GetForStudentAsync(int courseId, string studentId);
		Task<Assignment?> GetByIdForStudentAsync(int id, string studentId);
		Task<IReadOnlyList<Assignment>> GetWithSubmissionsForCourseAsync(int courseId);

        Task<IReadOnlyList<DashboardAssignmentDto>> GetActiveAssignmentsAsync();

    }
}
