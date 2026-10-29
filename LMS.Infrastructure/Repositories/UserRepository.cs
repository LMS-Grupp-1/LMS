using Domain.Contracts;
using LMS.Infrastructure.Data;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.Users;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetUsersWithCoursesAsync()
    {
        return await (from u in _context.Users
                      join c in _context.Courses on u.CourseId equals c.Id into courses
                      from c in courses.DefaultIfEmpty()
                      orderby u.FirstName
                      select new UserDto
                      {
                          Id = u.Id,
                          Email = u.Email ?? string.Empty,
                          FirstName = u.FirstName,
                          LastName = u.LastName,
                          Role = (from ur in _context.UserRoles
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where ur.UserId == u.Id
                                  select r.Name).FirstOrDefault() ?? string.Empty,
                          CourseId = u.CourseId,
                          CourseName = c != null ? c.Name : string.Empty
                      }).ToListAsync();
    }

    public async Task<IEnumerable<UserDto>> GetStudentsByCourseIdAsync(int courseId)
    {
        return await (from u in _context.Users
                      where u.CourseId == courseId
                            && (from ur in _context.UserRoles
                                join r in _context.Roles on ur.RoleId equals r.Id
                                where ur.UserId == u.Id && r.Name == UserRoles.Student
                                select r.Id).Any()
                      join c in _context.Courses on u.CourseId equals c.Id into courses
                      from c in courses.DefaultIfEmpty()
                      orderby u.FirstName
                      select new UserDto
                      {
                          Id = u.Id,
                          Email = u.Email ?? string.Empty,
                          FirstName = u.FirstName,
                          LastName = u.LastName,
                          Role = UserRoles.Student,
                          CourseId = u.CourseId,
                          CourseName = c != null ? c.Name : "Kurs finns inte"
                      }).ToListAsync();
    }

    public async Task<IReadOnlyList<UserLookupDto>> GetUsersByCourseIdAsync(int courseId, string? role = null)
    {
        var query = _context.Users.Where(u => u.CourseId == courseId);

        if (!string.IsNullOrEmpty(role))
        {
            // Only join and check roles table if a specific role is requested
            query = query.Where(u => _context.UserRoles
                .Where(ur => ur.UserId == u.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .Any(rName => rName == role));
        }

        return await query
            .OrderBy(u => u.FirstName)
            .Select(u => new UserLookupDto(
                u.Id,
                u.FirstName + " " + u.LastName
            ))
            .ToListAsync();
    }
}