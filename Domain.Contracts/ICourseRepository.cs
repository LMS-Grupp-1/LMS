using Domain.Models.Entities;
using LMS.Shared.DTOs;
using LMS.Shared.DTOs.CourseDtos;

namespace Domain.Contracts;

public interface ICourseRepository
{
	Task<IEnumerable<Course>> GetAllAsync(bool trackChanges);
    Task<IEnumerable<CourseDto>> GetAllWithCountsAsync(bool trackChanges);
    Task<IEnumerable<CourseLookupDto>> GetCourseLookupsAsync(bool trackChanges);
    Task<Course?> GetByIdAsync(int id, bool trackChanges);
	Task<Course?> GetForStudentAsync(string userId, bool trackChanges);

    Task<IEnumerable<DashboardCourseDto>> GetActiveCoursesAsync(int? count);

    void CreateCourse(Course course);
	void UpdateCourse(Course course);
	void DeleteCourse(Course course);
}
