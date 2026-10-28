using Domain.Models.Entities;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.DashboardDtos;
using LMS.Shared.DTOs.AssignmentDtos;

namespace Domain.Contracts
{
	public interface IAssignmentRepository : IBaseRepository<Assignment>
	{		
		Task<IReadOnlyList<Assignment>> GetForStudentAsync(int courseId, string studentId);
		Task<Assignment?> GetByIdForStudentAsync(int id, string studentId);
		Task<IReadOnlyList<Assignment>> GetWithSubmissionsForCourseAsync(int courseId);				
		Task<IReadOnlyList<TeacherAssignmentDto>> GetForModuleAsync(int moduleId);		
		Task<bool> HasSubmissionsAsync(int assignmentId);

		Task<IReadOnlyList<TeacherDashboardAssignmentDto>> GetActiveAssignmentsAsync(int? count = null);
		Task<AssignmentStatisticsDto?> GetAssignmentStatisticsForCourse(int courseId);

    }
}
