using LMS.Shared.DTOs.DashboardDtos;

namespace Service.Contracts
{
	public interface IDashboardService
	{
		Task<StudentDashboardDto> GetStudentDashboardAsync(
			string userId,
			CancellationToken cancellationToken);

        Task<IReadOnlyList<TeacherDashboardCourseDto>> GetActiveCoursesAsync(int? count = null);
        Task<IReadOnlyList<TeacherDashboardActivityDto>> GetUpcomingActivitiesAsync(int? count = null);
        Task<IReadOnlyList<TeacherDashboardAssignmentDto>> GetActiveAssignmentsAsync(int? count = null);
        Task<IReadOnlyList<TeacherDashboardSubmissionDto>> GetLatestSubmissionsAsync(DateTime? afterDate = null);
    }
}
