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
}