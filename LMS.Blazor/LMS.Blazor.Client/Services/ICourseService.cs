using LMS.Shared.DTOs.CourseDtos;

namespace LMS.Blazor.Client.Services
{
	public interface ICourseService
	{
		Task<StudentCourseDto?> GetMyCourseAsync(CancellationToken cancellationToken = default);
	}
}
