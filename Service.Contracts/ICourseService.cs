using LMS.Shared.DTOs.CourseDtos;

namespace Service.Contracts;

public interface ICourseService
{
	Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
	Task<CourseDto?> GetCourseByIdAsync(int id);
	Task<CourseDto> CreateCourseAsync(CourseCreateDto courseCreateDto);
	Task UpdateCourseAsync(int id, CourseUpdateDto courseUpdateDto);
	Task<StudentCourseDto> GetMyCourseAsync(string userId);
}
