namespace LMS.Shared.DTOs.DashboardDtos;

public record TeacherDashboardDto(
    IReadOnlyList<TeacherDashboardCourseDto>? ActiveCourses,
    IReadOnlyList<TeacherDashboardActivityDto>? UpcomingActivities,
    IReadOnlyList<TeacherDashboardAssignmentDto>? ActiveAssignments,
    IReadOnlyList<TeacherDashboardSubmissionDto>? LatestSubmissions
);

public record TeacherDashboardCourseDto(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    int StudentCount,
    int ModuleCount
);

public record TeacherDashboardActivityDto(
    int CourseId,
    int ActivityTypeId,
    string ActivityName,
    string CourseName,
    string ModuleName,
    DateTime StartDate,
    DateTime EndDate
);

// Active/Latest Assignments & Submissions
public record TeacherDashboardAssignmentDto(
    string CourseName,
    string AssignmentName,
    DateTime? Deadline
);

public record TeacherDashboardSubmissionDto(
    string AssignmentName,
    string StudentName,
    DateTimeOffset SubmittedAt
);
