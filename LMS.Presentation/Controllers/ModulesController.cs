using global::LMS.Shared.DTOs.ModulesDtos;
using LMS.Shared.Constants;
using LMS.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/teacher/courses/{courseId:int}/[controller]")]
public class ModulesController : ControllerBase
{
    private readonly IServiceManager _service;

    public ModulesController(IServiceManager service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetModules(int courseId)
    {
        var modules = await _service.ModuleService.GetModulesByCourseIdAsync(courseId);
        return Ok(modules);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> CreateModule(int courseId, [FromBody] CreateModuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage) = await _service.ModuleService.CreateModuleAsync(courseId, dto);

        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        // Returning Ok(true) to match your standard API response pattern
        return Ok(true);
    }

    [HttpPut("{moduleId:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> UpdateModule(int courseId, int moduleId, [FromBody] CreateModuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage) = await _service.ModuleService.UpdateModuleAsync(courseId, moduleId, dto);

        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(true);
    }

    [HttpDelete("{moduleId:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> DeleteModule(int courseId, int moduleId)
    {
        var (success, errorMessage) = await _service.ModuleService.DeleteModuleAsync(courseId, moduleId);

        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(true);
    }
}