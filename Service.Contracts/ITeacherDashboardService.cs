using LMS.Shared.DTOs;

namespace Service.Contracts;

public interface ITeacherDashboardService
{
    Task<IEnumerable<DashboardCourseDto>> GetActiveCoursesAsync(int? count = null);
    Task<IEnumerable<DashboardActivityDto>> GetActivitiesInDateRangeAsync(DateTime startDate, DateTime endDate, int? count = null);
    Task<IEnumerable<DashboardAssignmentDto>> GetActiveAssignmentsAsync();
    Task<IEnumerable<DashboardSubmissionDto>> GetLatestSubmissionsAsync(DateTime? afterDate = null);
}
