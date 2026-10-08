using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IUsersService> _usersService;
	private readonly Lazy<ICourseService> _courseService;

	public IAuthService AuthService => _authService.Value;
    public IUsersService UsersService => _usersService.Value;
	public ICourseService CourseService => _courseService.Value;

	public ServiceManager(Lazy<IAuthService> authService, Lazy<IUsersService> usersService, Lazy<ICourseService> courseService)
    {
        _authService = authService;
        _usersService = usersService;
        _courseService = courseService;
    }
}
