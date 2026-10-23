using LMS.Shared.DTOs.DashboardDtos;

namespace Service.Contracts
{
	public interface IDashboardService
	{
		Task<StudentDashboardDto> GetStudentDashboardAsync(
			string userId,
			CancellationToken cancellationToken);
	}
}
