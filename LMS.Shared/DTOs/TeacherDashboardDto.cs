
namespace LMS.Shared.DTOs;

public record TeacherDashboardDto(
    IEnumerable<DashboardCourseDto> ActiveCourses,
    IEnumerable<DashboardActivityDto> UpcomingActivities,
    IEnumerable<DashboardAssignmentDto> UpcomingAssignments
);

public record DashboardCourseDto(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    int StudentCount,
    int ModuleCount
);

public record DashboardActivityDto(
    int ActivityId,
    int ActivityTypeId,
    string Name,
    string CourseName,
    string ModuleName,
    DateTime StartDate,
    DateTime EndDate
);

// Active/Upcoming Assignments & Submissions
public record DashboardAssignmentDto(
    string AssignmentName,
    DateTime AssignmentDeadline,
    int RequiresSubmissions
);
public record DashboardSubmissionDto(
    string AssignmentName,
    string StudentName,
    DateTime SubmittedAt
);
