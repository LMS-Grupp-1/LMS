using Domain.Contracts;
using LMS.Shared.DTOs;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services;

public class TeacherDashboardService : ITeacherDashboardService
{
    private readonly IUnitOfWork _uow;

    public TeacherDashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<DashboardCourseDto>> GetActiveCoursesAsync(int? count = null)
    {
        return await _uow.Courses.GetActiveCoursesAsync(count);
    }

    public async Task<IReadOnlyList<DashboardActivityViewDto>> GetActivitiesInDateRangeAsync(DateTime startDate, DateTime endDate, int? count = null)
    {
        return await _uow.Activities.GetDateRangeActivitiesAsync(startDate, endDate, count);
    }

    public async Task<IReadOnlyList<DashboardAssignmentDto>> GetActiveAssignmentsAsync()
    {
        return await _uow.Assignments.GetActiveAssignmentsAsync();
    }

    public async Task<IReadOnlyList<DashboardSubmissionDto>?> GetLatestSubmissionsAsync(DateTime? afterDate = null)
    {
        return await _uow.Submissions.GetLatestAsync(afterDate);
    }

    // Statistics methods for the teacher course overview:
    public async Task<CourseStatisticsDto?> GetCourseStatisticsAsync(int courseId)
    {
        return await _uow.Courses.GetCourseStatisticsAsync(courseId);
    }

    public async Task<IReadOnlyList<ActivityOverviewDto>?> GetLatestActivitiesByCourseIdAsync(int courseId, int? count = null)
    {
        return await _uow.Activities.GetLatestActivitiesByCourseIdAsync(courseId, count);
    }

    public async Task<AssignmentStatisticsDto?> GetAssignmentStatisticsAsync(int courseId)
    {
        return await _uow.Assignments.GetAssignmentStatisticsForCourse(courseId);
    }

    public async Task<IReadOnlyList<SubmissionStatisticsDto>?> GetSubmissionStatisticsByCourseIdAsync(int courseId)
    {
        return await _uow.Submissions.GetSubmissionStatisticsByCourseIdAsync(courseId);
    }
}
