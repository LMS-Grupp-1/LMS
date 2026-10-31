using global::LMS.Shared.DTOs.ModulesDtos;
using LMS.Shared.DTOs.StatisticsDtos;
using LMS.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/modules")]
public class ModulesController : ControllerBase
{
    private readonly IServiceManager _service;

    public ModulesController(IServiceManager service)
    {
        _service = service;
    }

    [Authorize(Roles = UserRoles.Teacher)]
    [HttpGet("in-course/{courseId:int}")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetModules(int courseId)
    {
        var modules = await _service.ModuleService.GetModulesByCourseIdAsync(courseId);
        return Ok(modules);
    }

    [HttpGet("for-student")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetModulesForUser(CancellationToken cancellationToken = default)
    {
        // Extract userId from Claims in the HTTP layer
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var modules = await _service.ModuleService.GetModulesForUserAsync(userId, cancellationToken);
        return Ok(modules);
    }

    [HttpPost("in-course/{courseId:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> CreateModule(int courseId, [FromBody] CreateModuleDto dto)
    {
        // If something goes wrong, the service/database throws an exception 
        // (e.g., NotFoundException, ConflictException, or a general exception).
        // The teacher's ExceptionMiddleware automatically catches it and assigns the right status code (404, 409, 500)
        await _service.ModuleService.CreateModuleAsync(courseId, dto);

        return Ok(true);
    }

    [HttpPut("in-course/{courseId:int}/{moduleId:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> UpdateModule(int courseId, int moduleId, [FromBody] CreateModuleDto dto)
    {
        await _service.ModuleService.UpdateModuleAsync(courseId, moduleId, dto);

        return Ok(true);
    }

    [HttpDelete("in-course/{courseId:int}/{moduleId:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> DeleteModule(int courseId, int moduleId)
    {
        await _service.ModuleService.DeleteModuleAsync(courseId, moduleId);

        return Ok(true);
    }

    [HttpGet("{moduleId:int}/statistics")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<ModuleStatisticsDto?>> GetModuleStatistics(int moduleId, CancellationToken cancellationToken = default)
    {
        var dto = await _service.ModuleService.GetModuleStatisticsAsync(moduleId, cancellationToken);
        return (dto);
    }
}