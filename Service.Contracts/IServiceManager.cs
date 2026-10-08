namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    IUsersService UsersService { get; }
    IModuleService ModuleService { get; }
}