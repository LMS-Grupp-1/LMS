using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class CourseRepository(ApplicationDbContext context)
    : BaseRepository<Course>(context), ICourseRepository
{
    public async Task<IEnumerable<Course>> GetAllAsync(bool trackChanges) =>
        !trackChanges
            ? await _context.Courses.AsNoTracking().ToListAsync()
            : await _context.Courses.ToListAsync();

    // Lightweight DTO for lookups, only includes Id and Name
    public async Task<IEnumerable<CourseLookupDto>> GetCourseLookupsAsync(bool trackChanges)
    {
        IQueryable<Course> query = _context.Courses;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .Select(c => new CourseLookupDto(c.Id, c.Name))
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseDto>> GetAllWithCountsAsync(bool trackChanges)
    {
        IQueryable<Course> query = _context.Courses;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        // Speed up including related entities' counts
        return await query
            .Select(c => new CourseDto(
                c.Id,
                c.Name,
                c.Description,
                c.StartDate,
                c.EndDate,
                c.Students.Count(),
                c.Modules.Count()
            ))
            .ToListAsync();
    }

    public override Task<Course?> GetByIdAsync(int id)
    {
        return GetByIdAsync(id, trackChanges: false);
    }

    public Task<Course?> GetByIdAsync(int id, bool trackChanges)
    {
        if (trackChanges)
        {
            return _context.Courses
                .FirstOrDefaultAsync(course => course.Id == id);
        }
        return _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Id == id);
    }

    public async Task<Course?> GetForStudentAsync(string userId, bool trackChanges)
    {
        var query = _context.Courses
            .Include(c => c.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName))
            .Where(c => c.Students.Any(s => s.Id == userId));

        return !trackChanges
            ? await query.AsNoTracking().FirstOrDefaultAsync()
            : await query.FirstOrDefaultAsync();
    }

    public async Task<bool> NameExistsAsync(
        string name,
        int? excludedId = null)
    {
        var query = _context.Courses.AsQueryable();

        if (excludedId.HasValue)
        {
            int id = excludedId.Value;
            query = query.Where(course => course.Id != id);
        }
        string trimmedName = name.Trim();

        return await query.AnyAsync(course => course.Name == trimmedName);
    }

	public async Task<IReadOnlyList<ApplicationUser>> GetStudentsAsync(int courseId)
	{
		// Filter on the Student role, not only on CourseId:
		// a teacher can also have a CourseId, and must not be listed as a student.
		// Same role check as UserRepository.GetStudentsByCourseIdAsync.
		return await _context.Users
			.AsNoTracking()
			.Where(user => user.CourseId == courseId &&
				(from userRole in _context.UserRoles
				 join role in _context.Roles on userRole.RoleId equals role.Id
				 where userRole.UserId == user.Id && role.Name == UserRoles.Student
				 select role.Id).Any())
			.OrderBy(user => user.LastName)
			.ThenBy(user => user.FirstName)
			.ToListAsync();
	}
}
