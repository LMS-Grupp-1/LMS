using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LMS.Shared.DTOs.Users;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Authorize(Roles = "Teacher")]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IServiceManager _service;

    public UsersController(IServiceManager service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var (succeeded, errors) = await _service.UsersService.CreateUserAsync(dto);

        if (!succeeded)
        {
            // Add errors to ModelState so ASP.NET Core formats them into a standard ValidationProblemDetails
            foreach (var error in errors)
            {
                ModelState.AddModelError("UserCreation", error);
            }

            return ValidationProblem(ModelState); // Automatically returns ValidationProblemDetails (400)
        }

        return Ok(true);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _service.UsersService.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("students/{courseId}")]
    public async Task<IActionResult> GetUsersByCourseId(int courseId)
    {
        var users = await _service.UsersService.GetStudentsByCourseIdAsync(courseId);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _service.UsersService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        dto.Id = id;
        var result = await _service.UsersService.UpdateUserAsync(dto);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(true);
    }
}