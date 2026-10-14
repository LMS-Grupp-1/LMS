using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.DTOs.CourseDtos;
using System.Net.Http.Json;

namespace LMS.Blazor.Client.Services
{
	public class CourseService : ICourseService
	{
		private readonly IApiProxyClient _apiProxy;

		public CourseService(IApiProxyClient apiProxy)
		{
			_apiProxy = apiProxy;
		}

		public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
		{
			var courses = await _apiProxy.SendAsync<IEnumerable<CourseDto>>(
				HttpMethod.Get, "courses");

			return courses ?? Enumerable.Empty<CourseDto>();
		}

		public async Task<bool> CreateCourseAsync(CourseCreateDto dto)
		{
			using var content = JsonContent.Create(dto);

			await _apiProxy.SendAsync<CourseDto>(
				HttpMethod.Post, "courses", content);

			return true;
		}

		public async Task<bool> UpdateCourseAsync(int id, CourseUpdateDto dto)
		{
			using var content = JsonContent.Create(dto);

			await _apiProxy.SendAsync<object>(
			HttpMethod.Put, $"courses/{id}", content);
			return true;
		}

		public async Task<bool> DeleteCourseAsync(int id)
		{
			await _apiProxy.SendAsync<object>(
				HttpMethod.Delete, $"courses/{id}");

			return true;
		}

		public async Task<StudentCourseDto?> GetMyCourseAsync(CancellationToken cancellationToken = default)
		{
			return await _apiProxy.GetAsync<StudentCourseDto>("courses/my", cancellationToken);
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            return await _apiProxy.SendAsync<CourseDto>(HttpMethod.Get, $"courses/{id}");
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesWithCountsAsync()
        {
            try
            {
                var result = await _apiProxy.SendAsync<IEnumerable<CourseDto>>(
					HttpMethod.Get, "courses/with-counts");

                return result ?? Enumerable.Empty<CourseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching courses with counts: {ex.Message}");
                return Enumerable.Empty<CourseDto>();
            }
        }

		public async Task<IEnumerable<CourseLookupDto>> GetCourseLookupsAsync()
		{
            var result = await _apiProxy.SendAsync<IEnumerable<CourseLookupDto>>(
                HttpMethod.Get, "courses/lookups");

            return result ?? Enumerable.Empty<CourseLookupDto>();
		}
    }
}
