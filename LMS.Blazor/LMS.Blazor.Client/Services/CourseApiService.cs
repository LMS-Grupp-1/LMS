using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.Users;
using System.Net.Http.Json;
using System.Reflection.Metadata;

namespace LMS.Blazor.Client.Services
{
    public class CourseApiService : ICourseApiService
    {
        private readonly IApiProxyClient _apiProxy;

        public CourseApiService(IApiProxyClient apiProxy)
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
    }
}