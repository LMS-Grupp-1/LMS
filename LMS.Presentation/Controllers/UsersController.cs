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
        if (dto == null)
        {
            return BadRequest(new { message = "Request body is empty or invalid." });
        }

        var result = await _service.UsersService.CreateUserAsync(dto);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(true);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _service.UsersService.GetUsersAsync();
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