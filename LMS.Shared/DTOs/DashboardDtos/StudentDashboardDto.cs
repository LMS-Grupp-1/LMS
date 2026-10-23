using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.DocumentDtos;

namespace LMS.Shared.DTOs.DashboardDtos
{
	// Everything the student dashboard shows, fetched in one call.
	public record StudentDashboardDto(
		DashboardCourseDto Course,				
		DashboardModuleDto? CurrentModule,				
		IReadOnlyList<DashboardActivityDto> UpcomingActivities,		
		IReadOnlyList<StudentAssignmentDto> OverdueAssignments,		
		IReadOnlyList<StudentAssignmentDto> UpcomingDeadlines,		
		IReadOnlyList<StudentAssignmentDto> AssignmentsWithFeedback,		
		IReadOnlyList<DocumentDto> RecentDocuments);
}
