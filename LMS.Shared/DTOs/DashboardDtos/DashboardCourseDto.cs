namespace LMS.Shared.DTOs.DashboardDtos
{
	public record DashboardCourseDto(
	int Id,
	string Name,
	DateTime StartDate,
	DateTime EndDate);
}
