using global::LMS.Shared.DTOs.ModulesDtos;
using LMS.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Authorize(Roles = "Teacher")]
[ApiController]
[Route("api/teacher/courses/{courseId:int}/[controller]")]
public class ModulesController : ControllerBase
{
    private readonly IModuleService _moduleService;

    public ModulesController(IModuleService moduleService)
    {
        _moduleService = moduleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetModules(int courseId)
    {
        var modules = await _moduleService.GetModulesByCourseIdAsync(courseId);
        return Ok(modules);
    }

    [HttpPost]
    public async Task<IActionResult> CreateModule(int courseId, [FromBody] CreateModuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage) = await _moduleService.CreateModuleAsync(courseId, dto);

        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        // Returning Ok(true) to match your standard API response pattern
        return Ok(true);
    }
}