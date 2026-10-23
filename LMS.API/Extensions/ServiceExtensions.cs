using LMS.API.Services;
using LMS.Infrastructure.Repositories;
using LMS.Services;

namespace LMS.API.Extensions;

public static class ServiceExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
		services.AddScoped<IAssignmentRepository, AssignmentRepository>();
		services.AddScoped<ISubmissionRepository, SubmissionRepository>();
	}

    public static void AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddLazy<IAuthService>();

        services.AddScoped<IUsersService, UsersService>();
        services.AddLazy<IUsersService>();

        services.AddScoped<ICourseService, CourseService>();
        services.AddLazy<ICourseService>();

        services.AddScoped<IModuleService, ModuleService>();
        services.AddLazy<IModuleService>();

        services.AddScoped<IActivityService, ActivityService>();
        services.AddLazy<IActivityService>();

        services.AddScoped<IDocumentService, DocumentService>();
        services.AddLazy<IDocumentService>();

		services.AddScoped<IAssignmentService, AssignmentService>();
		services.AddLazy<IAssignmentService>();

        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddLazy<ISubmissionService>();

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddLazy<IDashboardService>();

        services.AddScoped<ITeacherDashboardService, TeacherDashboardService>();
        services.AddLazy<ITeacherDashboardService>();

        services.AddSingleton(TimeProvider.System);
	}
}
