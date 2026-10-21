using Azure.Storage.Blobs;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

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

		// Reference data must also be seeded in existing databases
		await SeedActivityTypesAsync(context, cancellationToken);

		//Existing databases keep their users and courses
		if (await context.Users.AnyAsync(cancellationToken))
		{
			if (environment.IsDevelopment())
			{
				await SeedTestModulesAsync(context, cancellationToken);
                await SeedTestActivitiesAsync(context, cancellationToken);
            }

            return;
        }

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

			if (environment.IsDevelopment())
			{
				await SeedTestModulesAsync(context, cancellationToken);
                await SeedTestActivitiesAsync(context, cancellationToken);
            }

			logger.LogInformation("Seed complete");
		}
		catch (Exception ex)
		{
			logger.LogError($"Data seed fail with message: {ex.Message}. Exception: {ex.InnerException}");
			throw;
		}
	}

	private async Task SeedActivityTypesAsync(
		ApplicationDbContext context,
		CancellationToken cancellationToken)
	{
		string[] requiredNames =
			[
			"Lecture",
			"E-Learning",
			"Exercise",
			"Assignment",
			"Other"
			];

		var existingNames = await context.ActivityTypes
				.AsNoTracking()
				.Select(type => type.Name)
				.ToListAsync(cancellationToken);

        var existingNameSet = existingNames
                .Select(name => name.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

		var missingTypes = requiredNames
				.Where(name => !existingNameSet.Contains(name))
				.Select(name => new ActivityType
				{
					Name = name
				})
				.ToList();

		if (missingTypes.Count == 0)
			return;

		context.ActivityTypes.AddRange(missingTypes);

		await context.SaveChangesAsync(cancellationToken);

		logger.LogInformation(
			"Seeded {Count} activity types.",
			missingTypes.Count);
    }

    private async Task SeedTestModulesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
	{
		string[] seedCourseNames =
			[
				"Lexicon LTU",
				"Lexicon GBG"
			];

		var courses = await context.Courses
			.AsNoTracking()
			.Where(course => seedCourseNames.Contains(course.Name))
			.ToListAsync(cancellationToken);

		int addedCount = 0;

		foreach (var course in courses)
		{
			bool hasModules = await context.Modules.AnyAsync(
				module => module.CourseId == course.Id,
				cancellationToken);

			if (hasModules)
				continue;

			if (course.StartDate >= course.EndDate)
			{
				logger.LogWarning(
					"Skipped test module for course {CourseId}: " +
					"the course period is too short or invalid.",
					course.Id);

				continue;
			}

			context.Modules.Add(new Domain.Models.Entities.Module
			{
				CourseId = course.Id,
				Name = course.Name == "Lexicon LTU"
					? "Grundläggande C#" : "HTML och CSS",
				Description = "Modul för att testa aktiviteter och tidsvalidering.",
				StartDate = course.StartDate,
				EndDate = course.EndDate,
			});

			addedCount++;
		}
		if (addedCount == 0)
			return;

		await context.SaveChangesAsync(cancellationToken);

		logger.LogInformation(
			"Seeded {Count} test modules.",
			addedCount);
	}

    private async Task SeedTestActivitiesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var modules = await context.Modules
            .AsNoTracking()
            .Where(module => (module.Course.Name == "Lexicon LTU" && 
				module.Name == "Grundläggande C#") || 
				(module.Course.Name == "Lexicon GBG" && 
				module.Name == "HTML och CSS"))
            .ToListAsync(cancellationToken);

		var activityTypes = await context.ActivityTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

		var lectureType = activityTypes.FirstOrDefault(type => string.Equals(
			type.Name.Trim(),
			"Lecture",
			StringComparison.OrdinalIgnoreCase));

        var exerciseType = activityTypes.FirstOrDefault(type => string.Equals(
            type.Name.Trim(),
            "Exercise",
            StringComparison.OrdinalIgnoreCase));

		if (lectureType is null || exerciseType is null)
		{
			throw new InvalidOperationException(
				"Lecture and Exercise activity types must be seeded " +
				"before test activities.");
		}

        int addedCount = 0;

        foreach (var module in modules)
        {
            bool hasActivities = await context.Activities.AnyAsync(
                activity => activity.ModuleId == module.Id,
                cancellationToken);

            if (hasActivities)
                continue;

			DateTime firstStart = module.StartDate.Date.AddHours(9);

            if (firstStart < module.StartDate)
            {
                firstStart = module.StartDate;
            }
			 
			DateTime firstEnd = firstStart.AddHours(1);
			DateTime secondStart = firstEnd.AddHours(1);
			DateTime secondEnd = secondStart.AddHours(1);

			if (firstStart.Date < module.StartDate.Date ||
				secondEnd.Date > module.EndDate.Date)
			{
				logger.LogWarning(
					"Skipped test activities for module {ModuleId}: " +
					"the module period cannot contain the test schedule.",
					module.Id);

				continue;
			}

            context.Activities.AddRange(new Activity
            {
                ModuleId = module.Id,
				ActivityTypeId = lectureType.Id,
                Name = "Introduktionsföreläsning",
                Description = "Genomgång av modulens innehåll och mål.",
                StartDate = firstStart,
                EndDate = firstEnd
            },
			new Activity
			{
                ModuleId = module.Id,
                ActivityTypeId = exerciseType.Id,
                Name = "Praktisk övning",
                Description = "Övning kopplad till introduktionsföreläsningen.",
                StartDate = secondStart,
                EndDate = secondEnd
            });

			addedCount += 2;
        }
        if (addedCount == 0)
            return;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded {Count} test activities.",
            addedCount);
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