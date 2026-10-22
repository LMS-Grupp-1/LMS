using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;
public class CourseRepository(ApplicationDbContext context) : ICourseRepository
{
    private readonly ApplicationDbContext _context = context;

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

    public async Task<Course?> GetByIdAsync(int id, bool trackChanges = false) =>
        !trackChanges
            ? await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id)
            : await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);

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

    public async Task<IEnumerable<DashboardCourseDto>> GetActiveCoursesAsync(int? count = null)
    {
        var today = DateTime.Today;

        IQueryable<Course> query = _context.Courses
            .Where(c => c.EndDate >= today)
            .OrderByDescending(c => c.StartDate);

        if (count.HasValue && count.Value > 0)
        {
            query = query.Take(count.Value);
        }

        return await query
            .Select(c => new DashboardCourseDto(
                c.Id,
                c.Name,
                c.StartDate,
                c.EndDate,
                c.Students.Count,
                c.Modules.Count
            ))
            .ToListAsync();
    }

    public void CreateCourse(Course course) => _context.Courses.Add(course);

    public void UpdateCourse(Course course) => _context.Courses.Update(course);

    public void DeleteCourse(Course course) => _context.Courses.Remove(course);

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
