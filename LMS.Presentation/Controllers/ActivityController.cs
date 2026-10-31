using LMS.Shared.Constants;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.ModulesDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/activities/in-module/{moduleId:int}")]
public class ActivityController : ControllerBase
{
    private readonly IServiceManager _service;

    public ActivityController(IServiceManager service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivites(int moduleId)
    {
        var activites = await _service.ActivityService.GetActivitesByModuleIdAsync(moduleId);
        return Ok(activites);
    }

    [HttpGet("for-student")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivitesForUser(int moduleId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var activites = await _service.ActivityService.GetActivitesByModuleIdForUserAsync(moduleId, userId);
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

    [HttpGet("{activityId:int}/deletion-preview")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<ActivityDeletionPreviewDto>> GetDeletionPreview(
        int moduleId,
        int activityId)
    {
        var preview = await _service.ActivityService.GetDeletionPreviewAsync(
            moduleId,
            activityId);

        return Ok(preview);
    }
    

    [HttpDelete("{activityId:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> DeleteActivity(
        int moduleId,
        int activityId,
        [FromBody] ActivityDeleteRequestDto request)
    {
        await _service.ActivityService.DeleteActivityAsync(
            moduleId,
            activityId,
            request);

        return NoContent();
    }
}