using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;

namespace LMS.Services;

public class UsersService : IUsersService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public UsersService(UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        ICourseService courseService, 
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Role) || !await _roleManager.RoleExistsAsync(dto.Role))
        {
            return (false, new[] { $"Rollen '{dto.Role}' finns inte." });
        }

        if (dto.Role.Equals(UserRoles.Student, StringComparison.OrdinalIgnoreCase) && (!dto.CourseId.HasValue || dto.CourseId.Value <= 0))
        {
            return (false, new[] { "Student måste tilldelas en kurs." });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName?.Trim() ?? string.Empty,
            LastName = dto.LastName?.Trim() ?? string.Empty,
            CourseId = dto.CourseId
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return (false, createResult.Errors.Select(e => e.Description));
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!addToRoleResult.Succeeded)
        {
            // Compensation / Rollback: Delete the user if role assignment fails
            await _userManager.DeleteAsync(user);
            return (false, addToRoleResult.Errors.Select(e => e.Description));
        }

        return (true, Enumerable.Empty<string>());
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(UpdateUserDto dto)
    {
        // 1. Validate role-specific course rule
        if (dto.Role.Equals(UserRoles.Student, StringComparison.OrdinalIgnoreCase) && (!dto.CourseId.HasValue || dto.CourseId.Value <= 0))
        {
            return (false, new[] { "Student måste tilldelas en kurs." });
        }

        // 2. Find user
        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user == null)
        {
            return (false, new[] { "Användare hittades inte." });
        }

        // 3. Validate role existence
        if (string.IsNullOrWhiteSpace(dto.Role) || !await _roleManager.RoleExistsAsync(dto.Role))
        {
            return (false, new[] { $"Rollen '{dto.Role}' finns inte." });
        }

        // 4. Handle Role Updates first (if role changed)
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(dto.Role))
        {
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return (false, removeResult.Errors.Select(e => e.Description));
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!addResult.Succeeded)
            {
                return (false, addResult.Errors.Select(e => e.Description));
            }
        }

        // 5. Update User Properties last
        user.UserName = dto.Email;
        user.Email = dto.Email;
        user.FirstName = dto.FirstName?.Trim() ?? string.Empty;
        user.LastName = dto.LastName?.Trim() ?? string.Empty;
        user.CourseId = dto.CourseId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description));
        }

        return (true, Enumerable.Empty<string>());
    }

    public async Task<IEnumerable<UserDto>?> GetUsersAsync()
    {
        // Single optimized database query using mixed LINQ query & method syntax (1 DB call instead of N+1)
        var userDtos = await _unitOfWork.Users.GetUsersWithCoursesAsync();

        return userDtos;
    }

    public async Task<IEnumerable<UserDto>?> GetStudentsByCourseIdAsync(int courseId)
    {
        var userDtos = await _unitOfWork.Users.GetStudentsByCourseIdAsync(courseId);

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
}