using LMS.Shared.Constants;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Authorize(Roles = UserRoles.Teacher)]
[ApiController]
[Route("api/teacher/course-overview")]
public class TeacherCourseOverviewController : ControllerBase
{
    private readonly IServiceManager _service;

    public TeacherCourseOverviewController(IServiceManager service)
    {
        _service = service;
    }

    // Statistics methods for the teacher course overview:
    [HttpGet("course-statistics/{courseId}")]
    public async Task<ActionResult<CourseStatisticsDto?>> GetCourseStatistics([FromRoute] int courseId)
    {
        var statistics = await _service.TeacherCourseOverviewService.GetCourseStatisticsAsync(courseId);
        return Ok(statistics);
    }

    [HttpGet("latest-activities/{courseId}")]
    public async Task<ActionResult<IReadOnlyList<ActivityOverviewDto>>> GetLatestActivitiesByCourseId([FromRoute] int courseId, [FromQuery] int? count = null)
    {
        var activities = await _service.TeacherCourseOverviewService.GetLatestActivitiesByCourseIdAsync(courseId, count);
        return Ok(activities);
    }

    [HttpGet("assignment-statistics/{courseId}")]
    public async Task<ActionResult<AssignmentStatisticsDto?>> GetAssignmentStatistics([FromRoute] int courseId)
    {
        var statistics = await _service.TeacherCourseOverviewService.GetAssignmentStatisticsAsync(courseId);
        return Ok(statistics);
    }

    [HttpGet("submission-statistics/{courseId}")]
    public async Task<ActionResult<IReadOnlyList<SubmissionStatisticsDto>>> GetSubmissionStatisticsByCourseId([FromRoute] int courseId)
    {
        var statistics = await _service.TeacherCourseOverviewService.GetSubmissionStatisticsByCourseIdAsync(courseId);
        return Ok(statistics);
    }
}