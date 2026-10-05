using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LMS.Shared.DTOs.Users;
using Service.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;

[Authorize(Roles = "Teacher")]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Request body is empty or invalid." });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = dto.Name
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (_roleManager != null)
            {
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                }
                await _userManager.AddToRoleAsync(user, dto.Role);
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Course))
        {
            // Add your course association logic here 
            // e.g., finding the course and adding the user to a join table, 
            // or saving it depending on how your LMS database is structured.
        }

        return Ok(new { message = "User created successfully!" });
    }
}