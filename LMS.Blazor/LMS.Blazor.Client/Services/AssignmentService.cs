using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.DTOs.AssignmentDtos;

namespace LMS.Blazor.Client.Services
{
	public class AssignmentService(IApiProxyClient apiProxy) : IAssignmentService
	{
		private readonly IApiProxyClient _apiProxy = apiProxy;

		public async Task<IReadOnlyList<StudentAssignmentDto>?> GetMyAssignmentsAsync(CancellationToken cancellationToken = default)
		{
			return await _apiProxy.GetAsync<List<StudentAssignmentDto>>("assignments/my", cancellationToken);
		}
	}
}
