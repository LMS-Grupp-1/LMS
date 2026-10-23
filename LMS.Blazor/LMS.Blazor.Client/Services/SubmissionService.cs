using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.DTOs.AssignmentDtos;

namespace LMS.Blazor.Client.Services
{
	public class SubmissionService(IApiProxyClient apiProxy) : ISubmissionService
	{
		public async Task<IReadOnlyList<TeacherSubmissionRowDto>?> GetForCourseAsync(
			int courseId,
			CancellationToken cancellationToken = default)
		{
			return await apiProxy.GetAsync<List<TeacherSubmissionRowDto>>(
				$"courses/{courseId}/submissions", cancellationToken);
		}
	}
}
