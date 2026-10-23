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
    public async Task<ActionResult<IEnumerable<DashboardCourseDto>>> GetActiveCourses([FromQuery] int? count = 3)
    {
        var courses = await _service.TeacherDashboardService.GetActiveCoursesAsync(count);
        return Ok(courses);
    }

    [HttpGet("upcoming-activities")]
    public async Task<ActionResult<IEnumerable<DashboardActivityDto>>> GetActivitiesInDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] int? count = 10)
    {
        var activities = await _service.TeacherDashboardService.GetActivitiesInDateRangeAsync(startDate, endDate, count);
        return Ok(activities);
    }

    [HttpGet("active-assignments")]
    public async Task<ActionResult<IEnumerable<DashboardAssignmentDto>>> GetActiveAssignments()
    {
        var assignments = await _service.TeacherDashboardService.GetActiveAssignmentsAsync();
        return Ok(assignments);
    }

    [HttpGet("latest-submissions")]
    public async Task<ActionResult<IEnumerable<DashboardSubmissionDto>>> GetLatestSubmissions([FromQuery] DateTime? afterDate = null)
    {
        var submissions = await _service.TeacherDashboardService.GetLatestSubmissionsAsync(afterDate);
        return Ok(submissions);
    }
}