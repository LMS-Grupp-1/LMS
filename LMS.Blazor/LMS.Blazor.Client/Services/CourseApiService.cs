using LMS.Shared.DTOs.CourseDtos;
using System.Net.Http.Json;

namespace LMS.Blazor.Client.Services
{
    public class CourseApiService : ICourseApiService
    {
        private readonly HttpClient _http;

        public CourseApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            try
            {
                // Calling API-endpoint.
                var response = await _http.GetFromJsonAsync<IEnumerable<CourseDto>>("api/courses");
                return response ?? Enumerable.Empty<CourseDto>();
            }
            catch
            {
                // Empty list, no crasch
                return Enumerable.Empty<CourseDto>();
            }
        }
        public async Task<bool> CreateCourseAsync(CourseCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/courses", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCourseAsync(int id, CourseUpdateDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/courses/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/courses/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}