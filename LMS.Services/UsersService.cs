using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
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

	public async Task CreateUserAsync(CreateUserDto dto)
	{
		// Validate before anything is changed, so no transaction is started for invalid input.
		if (string.IsNullOrWhiteSpace(dto.Role) || !await _roleManager.RoleExistsAsync(dto.Role))
		{
			throw new EntityNotFoundException("Role", dto.Role);
		}

		if (dto.Role.Equals(UserRoles.Student, StringComparison.OrdinalIgnoreCase) && (!dto.CourseId.HasValue || dto.CourseId.Value <= 0))
		{
			throw new BadRequestException("Student must be assigned to a course.");
		}

		var user = new ApplicationUser
		{
			UserName = dto.Email,
			Email = dto.Email,
			FirstName = dto.FirstName?.Trim() ?? string.Empty,
			LastName = dto.LastName?.Trim() ?? string.Empty,
			CourseId = dto.CourseId
		};

		// The user is only kept if the role could also be set, so there are never users without a role.
		await _unitOfWork.ExecuteInTransactionAsync(async () =>
		{
			ThrowIfFailed(await _userManager.CreateAsync(user, dto.Password));
			ThrowIfFailed(await _userManager.AddToRoleAsync(user, dto.Role));
		});
	}

	public async Task UpdateUserAsync(UpdateUserDto dto)
	{
		// 1. Validate before anything is changed, so no transaction is started for invalid input.
		if (string.IsNullOrWhiteSpace(dto.Role) || !await _roleManager.RoleExistsAsync(dto.Role))
		{
			throw new EntityNotFoundException("Role", dto.Role);
		}

		if (dto.Role.Equals(UserRoles.Student, StringComparison.OrdinalIgnoreCase) && (!dto.CourseId.HasValue || dto.CourseId.Value <= 0))
		{
			throw new BadRequestException("Student must be assigned to a course.");
		}

		var user = await _userManager.FindByIdAsync(dto.Id)
			?? throw new EntityNotFoundException("User", dto.Id);

		// 2. Roles and user details are saved together: if any step fails, nothing is saved.
		await _unitOfWork.ExecuteInTransactionAsync(async () =>
		{
			// A user has exactly one role, so every role except the chosen one is removed.
			var currentRoles = await _userManager.GetRolesAsync(user);
			var rolesToRemove = currentRoles
				.Where(role => !role.Equals(dto.Role, StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (rolesToRemove.Count > 0)
			{
				ThrowIfFailed(await _userManager.RemoveFromRolesAsync(user, rolesToRemove));
			}

			if (!currentRoles.Contains(dto.Role, StringComparer.OrdinalIgnoreCase))
			{
				ThrowIfFailed(await _userManager.AddToRoleAsync(user, dto.Role));
			}

			user.UserName = dto.Email;
			user.Email = dto.Email;
			user.FirstName = dto.FirstName?.Trim() ?? string.Empty;
			user.LastName = dto.LastName?.Trim() ?? string.Empty;
			user.CourseId = dto.CourseId;

			ThrowIfFailed(await _userManager.UpdateAsync(user));
		});
	}

	// Turns failed ASP.NET Core Identity results into a 400 with all error descriptions.
	// Throwing inside ExecuteInTransactionAsync also rolls the transaction back.
	private static void ThrowIfFailed(IdentityResult result)
	{
		if (!result.Succeeded)
		{
			throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
		}
	}

	public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        // Single optimized database query using mixed LINQ query & method syntax (1 DB call instead of N+1)
        return await _unitOfWork.Users.GetUsersWithCoursesAsync();
    }

    public async Task<IEnumerable<UserDto>> GetStudentsByCourseIdAsync(int courseId)
    {
        return await _unitOfWork.Users.GetStudentsByCourseIdAsync(courseId);
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) throw new EntityNotFoundException("User", id);

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