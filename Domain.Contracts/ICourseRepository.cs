using Domain.Models.Entities;
using LMS.Shared.DTOs;
using LMS.Shared.DTOs.CourseDtos;

namespace Domain.Contracts;

public interface ICourseRepository
{
	Task<IEnumerable<Course>> GetAllAsync(bool trackChanges);
    Task<IEnumerable<CourseDto>> GetAllWithCountsAsync(bool trackChanges);
    Task<IReadOnlyList<CourseLookupDto>?> GetCourseLookupsAsync();
	Task<IReadOnlyList<DashboardCourseDto>> GetActiveCoursesAsync(int? count = null);
    Task<Course?> GetByIdAsync(int id, bool trackChanges);
	Task<Course?> GetForStudentAsync(string userId, bool trackChanges);
	Task<bool> NameExistsAsync(string name, int? excludedId = null);
	Task AddAsync(Course model);
	void Update(Course model);
	void Delete(Course model);
	Task<IReadOnlyList<ApplicationUser>> GetStudentsAsync(int courseId);
	Task<CourseStatisticsDto?> GetCourseStatisticsAsync(int courseId);
}
