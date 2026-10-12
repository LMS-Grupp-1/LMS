using Microsoft.AspNetCore.Mvc.Testing;

namespace LMS.API.Tests;

/// <summary>
/// Verifies that the API's CORS policy only allows the configured BFF origin.
/// Sends the same preflight (OPTIONS) request a browser sends before a
/// cross-origin call, and checks the Access-Control-Allow-Origin header.
/// </summary>
/// <remarks>
/// These tests verify the CORS code in LMS.API (ConfigureCors and UseCors),
/// not the origin values in appsettings or Azure. Those differ per
/// environment and are verified in each environment.
/// </remarks>
public class CorsTests : IClassFixture<LmsApiFactory>
{
	private readonly HttpClient _client;

	// xUnit creates one LmsApiFactory for all tests in this class (IClassFixture),
	// so the API is started once and shared between the tests.
	public CorsTests(LmsApiFactory factory)
	{
		// Use https to avoid UseHttpsRedirection() redirecting the request
		// before the CORS middleware runs
		_client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			BaseAddress = new Uri("https://localhost")
		});
	}

	/// <summary>
	/// The BFF origin must get the Access-Control-Allow-Origin header
	/// with its own origin as value, not a wildcard (*).
	/// </summary>
	[Fact]
	public async Task Preflight_FromAllowedOrigin_ReturnsAllowOriginHeader()
	{
		var response = await SendPreflightAsync(LmsApiFactory.AllowedOrigin);

		// The header must exist
		Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));

		// and contain exactly the allowed origin. Fails if AllowAnyOrigin() is used,
		// since the value is then "*"
		Assert.Equal(LmsApiFactory.AllowedOrigin, values.Single());
	}

	/// <summary>
	/// An unknown origin must not get the Access-Control-Allow-Origin header.
	/// A missing header is how the API denies the request; the browser then blocks it.
	/// </summary>
	[Fact]
	public async Task Preflight_FromUnknownOrigin_DoesNotReturnAllowOriginHeader()
	{
		var response = await SendPreflightAsync("https://example.com");

		// The status code is 204 for both allowed and denied origins,
		// so only the header is checked
		Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
	}

	/// <summary>
	/// Sends the same OPTIONS request a browser sends before a cross-origin POST.
	/// </summary>
	/// <param name="origin">The origin the request pretends to come from.</param>
	private Task<HttpResponseMessage> SendPreflightAsync(string origin)
	{
		var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
		request.Headers.Add("Origin", origin);
		request.Headers.Add("Access-Control-Request-Method", "POST");
		
		return _client.SendAsync(request, TestContext.Current.CancellationToken);
	}
}