using Domain.Models.Entities;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.DashboardDtos;

namespace Domain.Contracts
{
	public interface IAssignmentRepository
	{		
		Task<IReadOnlyList<Assignment>> GetForStudentAsync(int courseId, string studentId);
		Task<Assignment?> GetByIdForStudentAsync(int id, string studentId);
		Task<IReadOnlyList<Assignment>> GetWithSubmissionsForCourseAsync(int courseId);

        Task<IReadOnlyList<TeacherDashboardAssignmentDto>> GetActiveAssignmentsAsync(int? count = null);
		Task<AssignmentStatisticsDto?> GetAssignmentStatisticsForCourse(int courseId);

    }
}
