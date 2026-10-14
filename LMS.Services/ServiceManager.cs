using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IUsersService> _usersService;
	private readonly Lazy<ICourseService> _courseService;
    private readonly Lazy<IModuleService> _moduleService;
    private readonly Lazy<IActivityService> _activityService;
    
	public IAuthService AuthService => _authService.Value;
    public IUsersService UsersService => _usersService.Value;
	public ICourseService CourseService => _courseService.Value;
    public IModuleService ModuleService => _moduleService.Value;
    public IActivityService ActivityService => _activityService.Value;
    
	public ServiceManager(Lazy<IAuthService> authService, 
		Lazy<IUsersService> usersService, 
        Lazy<ICourseService> courseService, 
		Lazy<IModuleService> moduleService,
		Lazy<IActivityService> activityService)
    {
        _authService = authService;
        _usersService = usersService;
		_courseService = courseService;
        _moduleService = moduleService;
        _activityService = activityService;
    }
}
