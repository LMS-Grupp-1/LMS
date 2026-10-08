using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;


internal class DataSeedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly ILogger<DataSeedService> logger;
    private UserManager<ApplicationUser> userManager = null!;
    private RoleManager<IdentityRole> roleManager = null!;
    private string _password = null!;
    private const string DemoRole = "Teacher";
    private const string DefaultUserEmail = "admin@lms.com";
    private const string NameForDefaultUser = "Admin";

    public DataSeedService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DataSeedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        if (!env.IsDevelopment()) return;

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
                            ?? throw new ArgumentNullException();

        await SeedCoursesAsync(context, cancellationToken);

        if (await context.Users.AnyAsync(cancellationToken)) return;

        userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()
                            ?? throw new ArgumentNullException();

        roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()
                            ?? throw new ArgumentNullException();

        _password = configuration["password"]!;
        ArgumentNullException.ThrowIfNull(_password, nameof(_password));

        try
        {
            await CreateRolesAsync([DemoRole]);
            await CreateDefaultUserAsync();
            logger.LogInformation("Seed complete");
        }
        catch (Exception ex)
        {
            logger.LogError($"Data seed fail with message: {ex.Message}. Exceeption: {ex.InnerException}");
            throw;
        }
    }

    private async Task SeedCoursesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (!await context.Courses.AnyAsync(cancellationToken))
        {
            var courses = new List<Course>
        {
            new Course
            {
                Name = "C# and .NET Core Development",
                Description = "Learn modern backend development using C#, ASP.NET Core, and Entity Framework Core.",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(2)
            },
            new Course
            {
                Name = "Frontend Web Development with Blazor",
                Description = "Build interactive single-page web applications using Blazor WebAssembly and C#.",
                StartDate = DateTime.Today.AddDays(7),
                EndDate = DateTime.Today.AddMonths(3)
            }
        };

            await context.Courses.AddRangeAsync(courses, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Course seed complete");
        }
    }
    private async Task CreateRolesAsync(string[] rolenames)
    {
        foreach (string rolename in rolenames)
        {
            if (await roleManager.RoleExistsAsync(rolename)) continue;
            var role = new IdentityRole { Name = rolename };
            var res = await roleManager.CreateAsync(role);

            if (!res.Succeeded) throw new Exception
                    (string.Join("\n", res.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }
    }
    private async Task CreateDefaultUserAsync()
    {
        var user = new ApplicationUser
        {
            Email = DefaultUserEmail,
            UserName = DefaultUserEmail,
            Name = NameForDefaultUser
        };

        await CreateUserAsync(user, DemoRole);
    }

    private async Task CreateUserAsync(ApplicationUser user, string role)
    {
        var result = await userManager.CreateAsync(user, _password);

        if (!result.Succeeded)
            throw new Exception(string.Join("\n",
            result.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
            throw new Exception(string.Join("\n",
            roleResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}