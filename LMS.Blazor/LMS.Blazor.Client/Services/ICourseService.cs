using LMS.Shared.DTOs.CourseDtos;

namespace LMS.Blazor.Client.Services
{
	public interface ICourseService
	{
		Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
		Task<bool> CreateCourseAsync(CourseCreateDto dto);
		Task<bool> UpdateCourseAsync(int id, CourseUpdateDto dto);
		Task<bool> DeleteCourseAsync(int id);
		Task<StudentCourseDto?> GetMyCourseAsync(CancellationToken cancellationToken = default);
	}
}
