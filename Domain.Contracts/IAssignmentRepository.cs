using Domain.Models.Entities;

namespace Domain.Contracts
{
	public interface IAssignmentRepository
	{		
		Task<IReadOnlyList<Assignment>> GetForStudentAsync(int courseId, string studentId);
		Task<Assignment?> GetByIdForStudentAsync(int id, string studentId);
	}
}
