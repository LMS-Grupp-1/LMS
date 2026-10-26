using LMS.Shared.DTOs.CourseDtos;

namespace Service.Contracts;

public interface ITeacherCourseOverviewService
{
    Task<CourseStatisticsDto?> GetCourseStatisticsAsync(int courseId);
    Task<IReadOnlyList<ActivityOverviewDto>> GetLatestActivitiesByCourseIdAsync(int courseId, int? count = null);
    Task<AssignmentStatisticsDto?> GetAssignmentStatisticsAsync(int courseId);
    Task<IReadOnlyList<SubmissionStatisticsDto>> GetSubmissionStatisticsByCourseIdAsync(int courseId);
}
