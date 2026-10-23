
namespace LMS.Shared.DTOs;

public record CourseStatisticsDto(
    int StudentCount,
    int ModuleCount,
    int ActivityCount,
    int AssignmentCount
);

public record ActivityOverviewDto(
    int ActivityTypeId,
    string ActivityName,
    string ModuleName,
    DateTime StartDate,
    DateTime EndDate
);

// Active/Latest Assignments & Submissions
public record AssignmentStatisticsDto(
    int TotalCount,
    int RequiresSubmissionCount,
    int OtherCount
);

public record SubmissionStatisticsDto(
    string AssignmentName,
    DateTime Deadline,
    int SubmissionCount,
    int FeedbackCount
);
