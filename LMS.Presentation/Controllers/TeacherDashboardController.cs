using LMS.Shared.Constants;
using LMS.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Authorize(Roles = UserRoles.Teacher)]
[ApiController]
[Route("api/teacher-dashboard")]
public class TeacherDashboardController : ControllerBase
{
    private readonly IServiceManager _service;

    public TeacherDashboardController(IServiceManager service)
    {
        _service = service;
    }

    [HttpGet("active-courses")]
    public async Task<ActionResult<IReadOnlyList<DashboardCourseDto>>> GetActiveCourses([FromQuery] int? count = 3)
    {
        var courses = await _service.TeacherDashboardService.GetActiveCoursesAsync(count);
        return Ok(courses);
    }

    [HttpGet("upcoming-activities")]
    public async Task<ActionResult<IReadOnlyList<DashboardActivityViewDto>>> GetActivitiesInDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] int? count = 10)
    {
        var activities = await _service.TeacherDashboardService.GetActivitiesInDateRangeAsync(startDate, endDate, count);
        return Ok(activities);
    }

    [HttpGet("active-assignments")]
    public async Task<ActionResult<IReadOnlyList<DashboardAssignmentDto>>> GetActiveAssignments()
    {
        var assignments = await _service.TeacherDashboardService.GetActiveAssignmentsAsync();
        return Ok(assignments);
    }

    [HttpGet("latest-submissions")]
    public async Task<ActionResult<IReadOnlyList<DashboardSubmissionDto>>> GetLatestSubmissions([FromQuery] DateTime? afterDate = null)
    {
        var submissions = await _service.TeacherDashboardService.GetLatestSubmissionsAsync(afterDate);
        return Ok(submissions);
    }

    // Statistics methods for the teacher course overview:
    [HttpGet("course-statistics/{courseId}")]
    public async Task<ActionResult<CourseStatisticsDto?>> GetCourseStatistics([FromRoute] int courseId)
    {
        var statistics = await _service.TeacherDashboardService.GetCourseStatisticsAsync(courseId);
        return Ok(statistics);
    }

    [HttpGet("latest-activities/{courseId}")]
    public async Task<ActionResult<IReadOnlyList<ActivityOverviewDto>?>> GetLatestActivitiesByCourseId([FromRoute] int courseId, [FromQuery] int? count = null)
    {
        var activities = await _service.TeacherDashboardService.GetLatestActivitiesByCourseIdAsync(courseId, count);
        return Ok(activities);
    }

    [HttpGet("assignment-statistics/{courseId}")]
    public async Task<ActionResult<AssignmentStatisticsDto?>> GetAssignmentStatistics([FromRoute] int courseId)
    {
        var statistics = await _service.TeacherDashboardService.GetAssignmentStatisticsAsync(courseId);
        return Ok(statistics);
    }

    [HttpGet("submission-statistics/{courseId}")]
    public async Task<ActionResult<IReadOnlyList<SubmissionStatisticsDto>>> GetSubmissionStatisticsByCourseId([FromRoute] int courseId)
    {
        var statistics = await _service.TeacherDashboardService.GetSubmissionStatisticsByCourseIdAsync(courseId);
        return Ok(statistics);
    }
}