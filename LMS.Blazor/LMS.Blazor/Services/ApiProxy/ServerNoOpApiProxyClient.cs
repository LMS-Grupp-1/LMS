using LMS.Blazor.Client.Services.ApiProxy;

namespace LMS.Blazor.Services.ApiProxy;

// Client components are also created once by the server during prerendering.
// The browser implementation performs the actual same-origin proxy request.
public sealed class ServerNoOpApiProxyClient(ILogger<ServerNoOpApiProxyClient> logger)
    : IApiProxyClient
{
    public Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string endpoint,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        content?.Dispose();

        logger.LogDebug(
            "Skipping {Method} proxy request during server prerendering: {Endpoint}",
            method,
            endpoint);

        return Task.FromResult<TResponse?>(default);
    }
}
