namespace LMS.Blazor.Client.Services.ApiProxy;

public interface IApiProxyClient
{
    // The client takes ownership of content and disposes it after the request.
    Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string endpoint,
        HttpContent? content = null,
        CancellationToken cancellationToken = default);
}
