using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LMS.Shared.DTOs.Users;
using Service.Contracts;

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

        return Ok(new { message = "User created successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _service.UsersService.GetUsersAsync();
        return Ok(users);
    }
}