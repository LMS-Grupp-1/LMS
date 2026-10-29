using LMS.Shared.Constants;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.ModulesDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/teacher/modules/{moduleId:int}/activities")]
public class ActivityController : ControllerBase
{
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
    public async Task<IActionResult> CreateActivity(int moduleId, [FromBody] ActivityFormDto dto)
    {

        await _service.ActivityService.CreateActivityAsync(moduleId, dto);
        return Ok(true);
    }

    [HttpPut("{activityId:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> UpdateActivity(int moduleId, int activityId, [FromBody] ActivityFormDto dto)
    {
        await _service.ActivityService.UpdateActivityAsync(moduleId, activityId, dto);

        return Ok(true);
    }
}