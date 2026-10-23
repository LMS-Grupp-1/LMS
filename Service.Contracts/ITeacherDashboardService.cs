using LMS.Shared.DTOs;

namespace Service.Contracts;

public interface ITeacherDashboardService
{
    Task<IReadOnlyList<DashboardCourseDto>> GetActiveCoursesAsync(int? count = null);
    Task<IReadOnlyList<DashboardActivityViewDto>> GetActivitiesInDateRangeAsync(DateTime startDate, DateTime endDate, int? count = null);
    Task<IReadOnlyList<DashboardAssignmentDto>> GetActiveAssignmentsAsync();
    Task<IReadOnlyList<DashboardSubmissionDto>?> GetLatestSubmissionsAsync(DateTime? afterDate = null);

    // Statistics methods for the teacher course overview:
    Task<CourseStatisticsDto?> GetCourseStatisticsAsync(int courseId);
    Task<IReadOnlyList<ActivityOverviewDto>?> GetLatestActivitiesByCourseIdAsync(int courseId, int? count = null);
    Task<AssignmentStatisticsDto?> GetAssignmentStatisticsAsync(int courseId);
    Task<IReadOnlyList<SubmissionStatisticsDto>?> GetSubmissionStatisticsByCourseIdAsync(int courseId);
}
