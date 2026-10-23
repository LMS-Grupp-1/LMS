using LMS.Shared.DTOs.DashboardDtos;

namespace LMS.Blazor.Client.Services
{
	public interface IDashboardService
	{
		// Returns null during server prerendering; the component then waits for WebAssembly.
		Task<StudentDashboardDto?> GetStudentDashboardAsync(CancellationToken cancellationToken = default);
	}
}
