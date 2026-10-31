using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IUsersService> _usersService;
	private readonly Lazy<ICourseService> _courseService;
    private readonly Lazy<IModuleService> _moduleService;
    private readonly Lazy<IActivityService> _activityService;
    private readonly Lazy<IDocumentService> _documentService;
    private readonly Lazy<ISharedDocumentService> _sharedDocumentService;
    private readonly Lazy<IAssignmentService> _assignmentService;
    private readonly Lazy<ISubmissionService> _submissionService;
    private readonly Lazy<IDashboardService> _dashboardService;
    private readonly Lazy<ITeacherCourseOverviewService> _teacherCourseOverviewService;
    private readonly Lazy<IFeedbackService> _feedbackService;
    private readonly Lazy<INotificationService> _notificationService;


    public IAuthService AuthService => _authService.Value;
    public IUsersService UsersService => _usersService.Value;
	public ICourseService CourseService => _courseService.Value;
    public IModuleService ModuleService => _moduleService.Value;
    public IActivityService ActivityService => _activityService.Value;
    public IDocumentService DocumentService => _documentService.Value;
    public ISharedDocumentService SharedDocumentService => _sharedDocumentService.Value;
    public IAssignmentService AssignmentService => _assignmentService.Value;
	public ISubmissionService SubmissionService => _submissionService.Value;
	public IDashboardService DashboardService => _dashboardService.Value;
    public ITeacherCourseOverviewService TeacherCourseOverviewService => _teacherCourseOverviewService.Value;
    public IFeedbackService FeedbackService => _feedbackService.Value;
    public INotificationService NotificationService => _notificationService.Value;

    public ServiceManager(Lazy<IAuthService> authService, 
		Lazy<IUsersService> usersService, 
        Lazy<ICourseService> courseService, 
		Lazy<IModuleService> moduleService,
		Lazy<IActivityService> activityService,
        Lazy<IDocumentService> documentService,
        Lazy<ISharedDocumentService> sharedDocumentService,
        Lazy<IAssignmentService> assignmentService,
        Lazy<ISubmissionService> submissionService,
        Lazy<IDashboardService> dashboardService,
        Lazy<ITeacherCourseOverviewService> teacherCourseOverviewService,
        Lazy<IFeedbackService> feedbackService,
        Lazy<INotificationService> notificationService)
    {
        _authService = authService;
        _usersService = usersService;
		_courseService = courseService;
        _moduleService = moduleService;
        _activityService = activityService;
        _documentService = documentService;
        _sharedDocumentService = sharedDocumentService;
        _assignmentService = assignmentService;
        _submissionService = submissionService;
        _dashboardService = dashboardService;
        _teacherCourseOverviewService = teacherCourseOverviewService;
        _feedbackService = feedbackService;
        _notificationService = notificationService;
    }
}
