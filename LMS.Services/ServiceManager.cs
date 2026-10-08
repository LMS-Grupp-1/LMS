using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IUsersService> _usersService;
    private readonly Lazy<IModuleService> _moduleService;

    public IAuthService AuthService => _authService.Value;
    public IUsersService UsersService => _usersService.Value;
    public IModuleService ModuleService => _moduleService.Value;

    public ServiceManager(Lazy<IAuthService> authService, Lazy<IUsersService> usersService, Lazy<IModuleService> moduleService)
    {
        _authService = authService;
        _usersService = usersService;
        _moduleService = moduleService;
    }
}
