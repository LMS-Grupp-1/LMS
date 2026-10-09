namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    IUsersService UsersService { get; }
	ICourseService CourseService { get; }
    IModuleService ModuleService { get; }
}