using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Models.Entities;
using Service.Contracts;
using LMS.Shared.DTOs.Users;

namespace LMS.Services;

public class UsersService : IUsersService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICourseService _courseService;

    public UsersService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ICourseService courseService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _courseService = courseService;
    }
    public async Task<IdentityResult> CreateUserAsync(CreateUserDto dto)
    {
        if (string.IsNullOrEmpty(dto.Role) || !await _roleManager.RoleExistsAsync(dto.Role))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RoleNotFound",
                Description = "Role not found."
            });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName ?? string.Empty,
            LastName = dto.LastName ?? string.Empty,
            CourseId = dto.CourseId ?? null
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return result;

        var addToRoleResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!addToRoleResult.Succeeded)
        {
            // ROLLBACK: Delete the user if role assignment fails
            await _userManager.DeleteAsync(user);
            return addToRoleResult;
        }

        return IdentityResult.Success;
    }
    public async Task<IEnumerable<UserDto>?> GetUsersAsync()
    {
        var users = _userManager.Users.OrderBy(u => u.FirstName).ToList();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var courseName = string.Empty;
            if (user.CourseId != null)
            {
                var course = await _courseService.GetCourseByIdAsync(user.CourseId.Value);
                if (course != null)
                {
                    courseName = course.Name;
                }
            }
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? string.Empty,
                CourseId = user.CourseId,
                CourseName = courseName
            });
        }

        return userDtos;
    }
    public async Task<IEnumerable<UserDto>?> GetUsersByCourseIdAsync(int courseId)
    {
        var users = _userManager.Users.Where(u => u.CourseId == courseId).OrderBy(u => u.FirstName).ToList();

        var courseName = "Kurs finns inte";
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course != null)
        {
            courseName = course.Name;
        }

        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? string.Empty,
                CourseId = user.CourseId,
                CourseName = courseName
            });
        }

        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto { 
            Id = user.Id, 
            Email = user.Email ?? string.Empty, 
            FirstName = user.FirstName,
			LastName = user.LastName,
			Role = roles.FirstOrDefault() ?? string.Empty,
            CourseId = user.CourseId
        };
    }

    public async Task<IdentityResult> UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        if (string.IsNullOrEmpty(dto.Role) || !await _roleManager.RoleExistsAsync(dto.Role))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RoleNotFound",
                Description = "Role not found."
            });
        }
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(dto.Role))
        {
            // AspNetUsersRoles is a many-to-many relationship, so we need to remove the old role.
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded) return removeResult;
            }

            var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!addResult.Succeeded) return addResult;
        }

        user.UserName = dto.Email;
        user.Email = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.CourseId = dto.CourseId ?? null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return result;

        return IdentityResult.Success;
    }
}