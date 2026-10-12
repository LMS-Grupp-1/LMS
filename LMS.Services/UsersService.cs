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

    public UsersService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task<IdentityResult> CreateUserAsync(CreateUserDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Role))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                if (!roleResult.Succeeded) return roleResult;
            }
        }
        else
        {
            
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName ?? string.Empty,
            LastName = dto.LastName ?? string.Empty
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

        // ToDo: Add user to course if dto.Course is provided

        return IdentityResult.Success;
    }
    public async Task<IEnumerable<UserDto>?> GetUsersAsync()
    {
        var users = _userManager.Users.ToList();
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
                Role = roles.FirstOrDefault() ?? string.Empty
			});

            // ToDo: Get the course
        }

        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        // ToDo: Get the course

        return new UserDto { 
            Id = user.Id, 
            Email = user.Email ?? string.Empty, 
            FirstName = user.FirstName,
			LastName = user.LastName,
			Role = roles.FirstOrDefault() ?? string.Empty
        };
    }

    public async Task<IdentityResult> UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        user.UserName = dto.Email;
        user.Email = dto.Email;
        user.FirstName = dto.FirstName ?? string.Empty;
        user.LastName = dto.LastName ?? string.Empty;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return result;

        if (!string.IsNullOrEmpty(dto.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);

            if (!currentRoles.Contains(dto.Role))
            {
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded) return removeResult;
                }

                // Ensure the role exists before adding
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                    if (!roleResult.Succeeded) return roleResult;
                }

                var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!addResult.Succeeded) return addResult;
            }
        }

        // ToDo: Update user's course if dto.Course is provided

        return IdentityResult.Success;
    }
}