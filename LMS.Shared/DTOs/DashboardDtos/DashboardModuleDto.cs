namespace LMS.Shared.DTOs.DashboardDtos
{
	public record DashboardModuleDto(
	int Id,
	string Name,
	DateTime StartDate,
	DateTime EndDate);
}
