using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.DTOs.DashboardDtos;

namespace LMS.Blazor.Client.Services
{
	public class DashboardService(IApiProxyClient apiProxy) : IDashboardService
	{
		public async Task<StudentDashboardDto?> GetStudentDashboardAsync(CancellationToken cancellationToken = default)
		{
			return await apiProxy.GetAsync<StudentDashboardDto>("dashboard/student", cancellationToken);
		}
	}
}
