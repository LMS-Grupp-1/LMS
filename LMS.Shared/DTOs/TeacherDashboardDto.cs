
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
    int CourseId,
    int ActivityTypeId,
    string ActivityName,
    string CourseName,
    string ModuleName,
    DateTime StartDate,
    DateTime EndDate
);

// Active/Upcoming Assignments & Submissions
public record DashboardAssignmentDto(
    string CourseName,
    string AssignmentName,
    DateTime? Deadline
);

public record DashboardSubmissionDto(
    string AssignmentName,
    string StudentName,
    DateTime SubmittedAt
);
