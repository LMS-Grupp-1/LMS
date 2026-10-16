using Azure.Storage.Blobs;
using LMS.Infrastructure.Data;
using LMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;


internal class DataSeedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly ILogger<DataSeedService> logger;
	private readonly IHostEnvironment environment;
	private UserManager<ApplicationUser> userManager = null!;
    private RoleManager<IdentityRole> roleManager = null!;
    private string _password = null!;
	    
	private const string DefaultUserEmail = "admin@lms.com";
    

    public DataSeedService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DataSeedService> logger, IHostEnvironment environment)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.logger = logger;
        this.environment = environment;
    }

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = serviceProvider.CreateScope();

		ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// Creates the database if needed and applies pending migrations.
		// In Azure this is how the tables are created. Locally it replaces Update-Database.
		await context.Database.MigrateAsync(cancellationToken);

		// Local development only: Azurite starts empty. In Azure the container is created by Bicep.
		BlobContainerClient? container = null;
		if (environment.IsDevelopment())
		{
			container = scope.ServiceProvider.GetRequiredService<BlobContainerClient>();
			await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
		}

		// Seed only an empty database, so existing data is never changed.
		if (await context.Users.AnyAsync(cancellationToken)) return;

		// A new database means the old blobs have no matching rows, so start with an empty container.
		if (container is not null)
		{
			await ClearContainerAsync(container, cancellationToken);
		}

		userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

		_password = configuration["password"]!;
		ArgumentNullException.ThrowIfNull(_password, nameof(_password));

		try
		{
			await CreateRolesAsync(UserRoles.All);
			await CreateDefaultUserAsync();
						
			await CreateCourseWithStudentsAsync(context,
				new Course
				{
					Name = "Lexicon LTU",
					Description = "Fullstack .NET developer course.",
					StartDate = new DateTime(2026, 9, 1),
					EndDate = new DateTime(2026, 12, 18)
				},
				[
					("anna.andersson@lms.com", "Anna", "Andersson"),
					("erik.eriksson@lms.com", "Erik", "Eriksson"),
					("maria.svensson@lms.com", "Maria", "Svensson")
				]);

			await CreateCourseWithStudentsAsync(context,
				new Course
				{
					Name = "Lexicon GBG",
					Description = "Frontend developer course.",
					StartDate = new DateTime(2026, 10, 1),
					EndDate = new DateTime(2027, 1, 29)
				},
				[
					("johan.nilsson@lms.com", "Johan", "Nilsson"),
					("sara.lindberg@lms.com", "Sara", "Lindberg")
				]);

			logger.LogInformation("Seed complete");
		}
		catch (Exception ex)
		{
			logger.LogError($"Data seed fail with message: {ex.Message}. Exception: {ex.InnerException}");
			throw;
		}
	}

	private async Task ClearContainerAsync(BlobContainerClient container, CancellationToken cancellationToken)
	{
		var deleted = 0;
		await foreach (var blob in container.GetBlobsAsync(cancellationToken: cancellationToken))
		{
			await container.DeleteBlobIfExistsAsync(blob.Name, cancellationToken: cancellationToken);
			deleted++;
		}

		logger.LogInformation("Cleared {Count} blobs from {Container}", deleted, container.Name);
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
			FirstName = "Admin",
			LastName = "Teacher"			
		};

		await CreateUserAsync(user, UserRoles.Teacher);
	}

	private async Task CreateCourseWithStudentsAsync(
		ApplicationDbContext context, Course course, 
		(string Email, string FirstName, string LastName)[] students)
	{
		
		context.Courses.Add(course);
		await context.SaveChangesAsync();

		foreach (var (email, firstName, lastName) in students)
		{
			var user = new ApplicationUser
			{
				Email = email,
				UserName = email,				
				FirstName = firstName,
				LastName = lastName,
				CourseId = course.Id
			};

			await CreateUserAsync(user, UserRoles.Student);
		}
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