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

    public async Task<IEnumerable<DashboardActivityDto>> GetUpcomingActivitiesAsync()
    {
        // Logic for activities...
        return Enumerable.Empty<DashboardActivityDto>();
    }

    public async Task<IEnumerable<DashboardAssignmentDto>> GetRecentSubmissionsAsync()
    {
        // Logic for submissions...
        return Enumerable.Empty<DashboardAssignmentDto>();
    }
}
