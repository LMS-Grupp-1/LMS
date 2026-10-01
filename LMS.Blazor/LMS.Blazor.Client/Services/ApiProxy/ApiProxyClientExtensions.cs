namespace LMS.Blazor.Client.Services.ApiProxy;

public static class ApiProxyClientExtensions
{
    public static Task<TResponse?> GetAsync<TResponse>(
        this IApiProxyClient client,
        string endpoint,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<TResponse>(
            HttpMethod.Get,
            endpoint,
            cancellationToken: cancellationToken);
}
