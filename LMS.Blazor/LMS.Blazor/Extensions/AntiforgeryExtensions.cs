using LMS.Blazor.Client.Services.ApiProxy;
using Microsoft.AspNetCore.Antiforgery;

namespace LMS.Blazor.Extensions;

public static class AntiforgeryExtensions
{
    public static IServiceCollection AddBffAntiforgery(
        this IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = AntiforgeryProtocol.HeaderName;
            options.Cookie.Name = "__Host-LMS-Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        return services;
    }

    public static IEndpointConventionBuilder MapAntiforgeryTokenEndpoint(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                AntiforgeryProtocol.TokenEndpoint,
                (IAntiforgery antiforgery, HttpContext httpContext) =>
                {
                    var tokens = antiforgery.GetAndStoreTokens(httpContext);
                    httpContext.Response.Headers.CacheControl = "no-store";

                    return string.IsNullOrWhiteSpace(tokens.RequestToken)
                        ? Results.StatusCode(StatusCodes.Status500InternalServerError)
                        : Results.Ok(new AntiforgeryTokenDto(tokens.RequestToken));
                })
            .RequireAuthorization();
}
