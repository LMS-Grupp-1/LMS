namespace LMS.Shared.DTOs.DashboardDtos
{
	public record DashboardActivityDto(
	int Id,
	string Name,
	string ActivityType,
	DateTime StartDate,
	DateTime EndDate,
	int ModuleId,
	string ModuleName);
}
