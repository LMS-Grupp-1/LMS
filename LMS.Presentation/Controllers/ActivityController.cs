using LMS.Shared.DTOs.ActivityDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Shared.Constants;
using Service.Contracts;

namespace LMS.Presentation;

[ApiController]
[Authorize]
[Route("api/teacher/courses/Modules/{moduleId:int}/Activities")]
public class ActivityController : ControllerBase
{
    // private readonly ICourseService _courseService = serviceManager.CourseService;
    private readonly IServiceManager _service;

    public ActivityController(IServiceManager service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivites(int moduleId)
    {
        var activites = await _service.ActivityService.GetActivitesByModuleIdAsync(moduleId);
        return Ok(activites);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> CreateActivity(int moduleId, [FromBody] CreateActivityDto dto)
    {

        await _service.ActivityService.CreateActivityAsync(moduleId, dto);
        return NoContent();
    }
}