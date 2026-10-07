using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.DTOs.CourseDtos;

namespace LMS.Blazor.Client.Services
{
	public class CourseService : ICourseService
	{
		private readonly IApiProxyClient _apiProxy;

		public CourseService(IApiProxyClient apiProxy)
		{
			_apiProxy = apiProxy;
		}

		public async Task<StudentCourseDto?> GetMyCourseAsync(CancellationToken cancellationToken = default)
		{
			return await _apiProxy.GetAsync<StudentCourseDto>("courses/my", cancellationToken);
		}
	}
}
