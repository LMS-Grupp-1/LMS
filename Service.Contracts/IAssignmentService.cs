using LMS.Shared.DTOs.AssignmentDtos;

namespace Service.Contracts
{
	public interface IAssignmentService
	{
		Task<IReadOnlyList<StudentAssignmentDto>> GetMyAssignmentsAsync(string userId);
		
		// Teacher
		Task<IReadOnlyList<TeacherAssignmentDto>> GetForModuleAsync(int moduleId);
		Task CreateAsync(AssignmentFormDto dto);
		Task UpdateAsync(int id, AssignmentFormDto dto);
		Task DeleteAsync(int id);
	}
}
