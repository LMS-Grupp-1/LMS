using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IUsersService> _usersService;

    public IAuthService AuthService => _authService.Value;
    public IUsersService UsersService => _usersService.Value;

    public ServiceManager(Lazy<IAuthService> authService, Lazy<IUsersService> usersService)
    {
        _authService = authService;
        _usersService = usersService;
    }
}
