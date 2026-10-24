using Domain.Contracts;
using LMS.Shared.DTOs.CourseDtos;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services;

public class TeacherCourseOverviewService : ITeacherCourseOverviewService
{
    private readonly IUnitOfWork _uow;

    public TeacherCourseOverviewService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CourseStatisticsDto?> GetCourseStatisticsAsync(int courseId)
    {
        return await _uow.Courses.GetCourseStatisticsAsync(courseId);
    }

    public async Task<IReadOnlyList<ActivityOverviewDto>> GetLatestActivitiesByCourseIdAsync(int courseId, int? count = null)
    {
        return await _uow.Activities.GetLatestActivitiesByCourseIdAsync(courseId, count);
    }

    public async Task<AssignmentStatisticsDto?> GetAssignmentStatisticsAsync(int courseId)
    {
        return await _uow.Assignments.GetAssignmentStatisticsForCourse(courseId);
    }

    public async Task<IReadOnlyList<SubmissionStatisticsDto>> GetSubmissionStatisticsByCourseIdAsync(int courseId)
    {
        return await _uow.Submissions.GetSubmissionStatisticsByCourseIdAsync(courseId);
    }
}
