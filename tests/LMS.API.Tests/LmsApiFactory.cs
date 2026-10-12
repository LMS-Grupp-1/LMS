using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.API.Tests;

/// <summary>
/// Starts the real LMS.API in memory for integration tests.
/// Runs Program.cs as usual, but replaces configuration and services
/// that depend on the developer's machine (user secrets, LocalDB).
/// </summary>
public class LmsApiFactory : WebApplicationFactory<Program>
{
	/// <summary>
	/// Fake BFF origin used by the tests. It is never contacted,
	/// it is only sent as the value of the Origin header.
	/// </summary>
	public const string AllowedOrigin = "https://bff.test";

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		// Use a separate environment so appsettings.Development.json is not loaded.
		// The tests must not depend on a developer's local BFF port.
		builder.UseEnvironment("Testing");

		// User secrets are not loaded in tests, so the required JWT key is set here.
		// Without it, ValidateOnStart() on JwtSettings stops the API from starting.
		// The value does not need to be secret since no real tokens are issued.
		builder.UseSetting("JwtSettings:SecretKey", new string('x', 64));

		// Allow only the fake BFF origin. Equivalent to:
		// "Cors": { "AllowedOrigins": [ "https://bff.test" ] }
		builder.UseSetting("Cors:AllowedOrigins:0", AllowedOrigin);

		// Runs after Program.cs has registered all services
		builder.ConfigureTestServices(services =>
		{
			// Remove the hosted service that seeds the database on startup.
			// It requires LocalDB, which does not exist in GitHub Actions,
			// and the CORS tests do not need a database.
			// Matched by name since the class may be internal to LMS.API.
			var seeder = services.SingleOrDefault(
				d => d.ImplementationType?.Name == "DataSeedService");

			if (seeder is not null)
				services.Remove(seeder);
		});
	}
}