using LMS.API.Services;
using LMS.Infrastructure.Repositories;
using LMS.Services;

namespace LMS.API.Extensions;

public static class ServiceExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICourseRepository, CourseRepository>();
    }

    public static void AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddLazy<IAuthService>();

        services.AddScoped<ICourseService, CourseService>();
        services.AddLazy<ICourseService>();
      
        services.AddScoped<IUsersService, UsersService>();
        services.AddLazy<IUsersService>();

        services.AddScoped<ICourseService, CourseService>();
        services.AddLazy<ICourseService>();
    }
}
