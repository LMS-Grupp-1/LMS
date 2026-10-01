using LMS.Blazor.Services.Authentication.Tokens;
using LMS.Shared.DTOs.AuthDtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace LMS.Blazor.Services.Authentication;

public sealed class RemoteAuthenticationService(
    HttpClient httpClient,
    ITokenStorage tokenStorage,
    ILogger<RemoteAuthenticationService> logger)
{
    public async Task<RemoteLoginResult> LoginAsync(
        UserAuthDto credentials,
        HttpContext httpContext,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var loginResponse = await httpClient.PostAsJsonAsync(
                "api/auth/login",
                credentials,
                cancellationToken);

            if (loginResponse.StatusCode == HttpStatusCode.Unauthorized)
                return RemoteLoginResult.InvalidCredentials;

            if (!loginResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Remote login failed with status code {StatusCode}",
                    loginResponse.StatusCode);

                return RemoteLoginResult.ServiceError;
            }

            var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenDto>(cancellationToken);

            if (tokens is null ||
                string.IsNullOrWhiteSpace(tokens.AccessToken) ||
                string.IsNullOrWhiteSpace(tokens.RefreshToken) ||
                tokens.AccessTokenExpiresAtUtc <= DateTimeOffset.UtcNow ||
                tokens.RefreshTokenExpiresAtUtc <= tokens.AccessTokenExpiresAtUtc)
            {
                logger.LogWarning("Remote login returned an invalid token response");
                return RemoteLoginResult.ServiceError;
            }

            using var userRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            using var userResponse = await httpClient.SendAsync(userRequest, cancellationToken);
            if (!userResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "The remote API rejected a newly issued access token with status code {StatusCode}",
                    userResponse.StatusCode);

                return RemoteLoginResult.ServiceError;
            }

            var authenticatedUser = await userResponse.Content
                .ReadFromJsonAsync<AuthenticatedUserDto>(cancellationToken);

            if (authenticatedUser?.Claims is not { Count: > 0 })
            {
                logger.LogWarning("The remote API returned no claims for the authenticated user");
                return RemoteLoginResult.ServiceError;
            }

            var claims = authenticatedUser.Claims
                .Where(claim => !ProtocolClaimTypes.Contains(claim.Type))
                .Select(claim => new Claim(claim.Type, claim.Value))
                .ToList();

            var userId = claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning("The remote API token contains no name identifier claim");
                return RemoteLoginResult.ServiceError;
            }

            var sessionId = Guid.NewGuid().ToString("N");
            claims.Add(new Claim(ClaimTypes.Sid, sessionId));

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role);

            var properties = new AuthenticationProperties
            {
                AllowRefresh = false,
                IsPersistent = rememberMe,
                ExpiresUtc = tokens.RefreshTokenExpiresAtUtc
            };

            await tokenStorage.StoreTokensAsync(sessionId, userId, tokens);

            try
            {
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    properties);
            }
            catch
            {
                await tokenStorage.RemoveTokensAsync(sessionId);
                throw;
            }

            return RemoteLoginResult.Succeeded;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Remote login timed out");
            return RemoteLoginResult.ServiceError;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Could not reach the remote authentication API");
            return RemoteLoginResult.ServiceError;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "The remote authentication API returned invalid JSON");
            return RemoteLoginResult.ServiceError;
        }
    }

    private static readonly HashSet<string> ProtocolClaimTypes = new(StringComparer.Ordinal)
    {
        "aud",
        "exp",
        "iat",
        "iss",
        "nbf"
    };
}
