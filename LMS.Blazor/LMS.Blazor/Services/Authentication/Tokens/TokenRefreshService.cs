using LMS.Blazor.Services.Authentication.Tokens.Helpers;
using LMS.Shared.DTOs.AuthDtos;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace LMS.Blazor.Services.Authentication.Tokens;

public sealed class TokenRefreshService(
    IHttpClientFactory httpClientFactory,
    ITokenStorage tokenStorage,
    ILogger<TokenRefreshService> logger)
{
    private static readonly TimeSpan RefreshBeforeExpiration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan RefreshOperationTimeout = TimeSpan.FromSeconds(15);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _sessionLocks = new();

    public async Task<AccessTokenResult> GetValidAccessTokenAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var sessionId = principal.FindFirstValue(ClaimTypes.Sid);
        if (string.IsNullOrWhiteSpace(sessionId))
            return AccessTokenResult.Unauthorized;

        var evaluation = await EvaluateSessionAsync(sessionId);
        if (evaluation.CompletedResult is { } completedResult)
            return completedResult;

        // Det här låser per session, så bara en tråd åt gången kan
        // refresha samma sessions token. Det skyddar mot att flera samtidiga
        // anrop försöker uppdatera samma token och skriver över varandra.
        var sessionLock = _sessionLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
        await sessionLock.WaitAsync(cancellationToken);

        try
        {
            // Ett annat anrop kan ha uppdaterat token medan vi väntade på
            // låset, så sessionen måste kontrolleras igen.
            evaluation = await EvaluateSessionAsync(sessionId);
            if (evaluation.CompletedResult is { } resultAfterLock)
                return resultAfterLock;

            var sessionToRefresh = evaluation.SessionToRefresh
                ?? throw new InvalidOperationException(
                    "A session that requires refresh must contain token data.");

            // Refresh-token rotation changes server-side state. Once started it
            // must finish even if the browser navigates away and aborts the
            // request that triggered it, otherwise the newly rotated token can
            // be lost and the next request will be treated as unauthorized.
            return await RefreshTokensAsync(sessionId, sessionToRefresh);
        }
        finally
        {
            sessionLock.Release();
        }
    }

    private async Task<(TokenSession? SessionToRefresh, AccessTokenResult? CompletedResult)> EvaluateSessionAsync(string sessionId)
    {
        var session = await tokenStorage.GetSessionAsync(sessionId);

        if (session is null)
            return (null, AccessTokenResult.Unauthorized);

        var now = DateTimeOffset.UtcNow;

        if (session.Tokens.RefreshTokenExpiresAtUtc <= now)
        {
            await tokenStorage.RemoveTokensAsync(sessionId);
            return (null, AccessTokenResult.Unauthorized);
        }

        if (session.Tokens.AccessTokenExpiresAtUtc > now.Add(RefreshBeforeExpiration))
            return (null, AccessTokenResult.Success(session.Tokens.AccessToken));

        return (session, null);
    }

    private async Task<AccessTokenResult> RefreshTokensAsync(
        string sessionId,
        TokenSession session)
    {
        using var refreshOperationCancellation =
            new CancellationTokenSource(RefreshOperationTimeout);
        var cancellationToken = refreshOperationCancellation.Token;

        try
        {
            var httpClient = httpClientFactory.CreateClient(RemoteApiHttpClientNames.TokenRefresh);
            using var response = await httpClient.PostAsJsonAsync(
                "api/token/refresh",
                session.Tokens,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                await tokenStorage.RemoveTokensAsync(sessionId);
                return AccessTokenResult.Unauthorized;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Token refresh failed with status code {StatusCode}",
                    response.StatusCode);

                return AccessTokenResult.Unavailable;
            }

            var refreshedTokens = await response.Content.ReadFromJsonAsync<TokenDto>(cancellationToken);
            var now = DateTimeOffset.UtcNow;

            if (refreshedTokens is null ||
                string.IsNullOrWhiteSpace(refreshedTokens.AccessToken) ||
                string.IsNullOrWhiteSpace(refreshedTokens.RefreshToken) ||
                refreshedTokens.AccessTokenExpiresAtUtc <= now ||
                refreshedTokens.RefreshTokenExpiresAtUtc <= now)
            {
                logger.LogWarning("The remote API returned an invalid refresh response");
                return AccessTokenResult.Unavailable;
            }

            await tokenStorage.StoreTokensAsync(sessionId, session.UserId, refreshedTokens);
            logger.LogInformation("Refreshed the access token for an authenticated session");
            return AccessTokenResult.Success(refreshedTokens.AccessToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Token refresh timed out");
            return AccessTokenResult.Unavailable;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Could not reach the remote token endpoint");
            return AccessTokenResult.Unavailable;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "The remote token endpoint returned invalid JSON");
            return AccessTokenResult.Unavailable;
        }
    }
}
