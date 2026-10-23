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

    public async Task<IEnumerable<DashboardCourseDto>> GetActiveCoursesAsync(int? count = null)
    {
        return await _uow.Courses.GetActiveCoursesAsync(count);
    }

    public async Task<IEnumerable<DashboardActivityDto>> GetActivitiesInDateRangeAsync(DateTime startDate, DateTime endDate, int? count = null)
    {
        return await _uow.Activities.GetDateRangeActivitiesAsync(startDate, endDate, count);
    }

    public async Task<IEnumerable<DashboardAssignmentDto>> GetActiveAssignmentsAsync()
    {
        return await _uow.Assignments.GetActiveAssignmentsAsync();
    }

    public async Task<IEnumerable<DashboardSubmissionDto>> GetLatestSubmissionsAsync(DateTime? afterDate = null)
    {
        return await _uow.Submissions.GetLatestAsync(afterDate);
    }
}
