namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    IUsersService UsersService { get; }
	ICourseService CourseService { get; }
    IModuleService ModuleService { get; }
    IActivityService ActivityService { get; }
    IDocumentService DocumentService { get; }
	IAssignmentService AssignmentService { get; }
	ISubmissionService SubmissionService { get; }
	IDashboardService DashboardService { get; }

    ITeacherCourseOverviewService TeacherCourseOverviewService { get; }
}