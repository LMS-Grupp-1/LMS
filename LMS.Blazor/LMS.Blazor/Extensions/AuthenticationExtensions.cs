using LMS.Blazor.Components.Account;
using LMS.Blazor.Services.Authentication;
using LMS.Blazor.Services.Authentication.Tokens;
using LMS.Blazor.Services.Authentication.Tokens.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace LMS.Blazor.Extensions;

public static class AuthenticationExtensions
{
    private const string RemoteApiAddressConfigurationKey =
        "ReverseProxy:Clusters:remote-api:Destinations:primary:Address";

    public static IServiceCollection AddBffAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<AccountRedirectManager>();
        services.AddSingleton<ITokenStorage, TokenStorageService>();
        services.AddSingleton<TokenRefreshService>();

        var remoteApiBaseUrl = configuration[RemoteApiAddressConfigurationKey]
            ?? throw new InvalidOperationException(
                "The Remote API destination is not configured.");

        services.AddHttpClient<RemoteAuthenticationService>(httpClient =>
            httpClient.BaseAddress = new Uri(remoteApiBaseUrl));

        services.AddHttpClient(RemoteApiHttpClientNames.TokenRefresh, httpClient =>
            httpClient.BaseAddress = new Uri(remoteApiBaseUrl));

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(ConfigureCookie);
        services.AddAuthorization();

        return services;
    }

    private static void ConfigureCookie(CookieAuthenticationOptions options)
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = false;
        options.Events.OnValidatePrincipal = ValidatePrincipalAsync;
        options.Events.OnRedirectToLogin = context =>
            HandleRedirectAsync(
                context,
                StatusCodes.Status401Unauthorized);
        options.Events.OnRedirectToAccessDenied = context =>
            HandleRedirectAsync(
                context,
                StatusCodes.Status403Forbidden);
    }

    private static async Task ValidatePrincipalAsync(
        CookieValidatePrincipalContext context)
    {
        var sessionId = context.Principal?.FindFirstValue(ClaimTypes.Sid);
        var tokenStorage = context.HttpContext.RequestServices
            .GetRequiredService<ITokenStorage>();

        var session = string.IsNullOrWhiteSpace(sessionId)
            ? null
            : await tokenStorage.GetSessionAsync(sessionId);

        if (session is not null &&
            session.Tokens.RefreshTokenExpiresAtUtc > DateTimeOffset.UtcNow)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(sessionId) && session is not null)
        {
            await tokenStorage.RemoveTokensAsync(sessionId);
        }

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private static Task HandleRedirectAsync(
        RedirectContext<CookieAuthenticationOptions> context,
        int apiStatusCode)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = apiStatusCode;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }

        return Task.CompletedTask;
    }
}
