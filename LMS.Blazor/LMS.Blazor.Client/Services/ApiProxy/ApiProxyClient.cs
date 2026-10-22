using LMS.Shared.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace LMS.Blazor.Client.Services.ApiProxy;

public sealed class ApiProxyClient(HttpClient httpClient) : IApiProxyClient
{
    public async Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string endpoint,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        var proxyUri = GetProxyUri(endpoint);
        using var request = new HttpRequestMessage(method, proxyUri)
        {
            Content = content
        };

        if (RequiresAntiforgery(method))
        {
            var requestToken = await GetAntiforgeryTokenAsync(cancellationToken);
            request.Headers.Add(AntiforgeryProtocol.HeaderName, requestToken);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);

        EnsureAuthenticated(response);
        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.NoContent ||
            response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public async Task<(TResponse? Data, string? ErrorMessage)> SendAndHandleErrorAsync<TResponse>(
    HttpMethod method,
    string endpoint,
    HttpContent? content = null,
    CancellationToken cancellationToken = default)
    {
        var proxyUri = GetProxyUri(endpoint);
        using var request = new HttpRequestMessage(method, proxyUri) { Content = content };

        if (RequiresAntiforgery(method))
        {
            var requestToken = await GetAntiforgeryTokenAsync(cancellationToken);
            request.Headers.Add(AntiforgeryProtocol.HeaderName, requestToken);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        EnsureAuthenticated(response);

        // If it's a 400 Bad Request (or any error status code), read the custom error message returned by your controller
        if ((int)response.StatusCode == 400 || !response.IsSuccessStatusCode)
        {
            try
            {
                // Deserialize into our clean ProblemDetails model
                var problemDetails = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(cancellationToken: cancellationToken);

                // Check for field-level validation errors from [ApiController]
                if (problemDetails?.Errors is { Count: > 0 } errors)
                {
                    var validationMessages = string.Join(" ", errors.Values.SelectMany(messages => messages));
                    return (default, validationMessages);
                }

                // Check for standard ProblemDetails message from ExceptionMiddleware (Detail or Title)
                var message = problemDetails?.Detail ?? problemDetails?.Title;
                if (!string.IsNullOrEmpty(message))
                {
                    return (default, message);
                }

                return (default, $"Ett fel uppstod (Status: {(int)response.StatusCode}).");
            }
            catch
            {
                // Fallback if the response body isn't JSON
                var rawError = await response.Content.ReadAsStringAsync(cancellationToken);
                return (default, string.IsNullOrWhiteSpace(rawError) ? "Ett valideringsfel uppstod." : rawError);
            }
        }

        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
        {
            return (default, null);
        }

        var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        return (data, null);
    }

    private async Task<string> GetAntiforgeryTokenAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            AntiforgeryProtocol.TokenEndpoint,
            cancellationToken);

        EnsureAuthenticated(response);
        response.EnsureSuccessStatusCode();

        var token = await response.Content
            .ReadFromJsonAsync<AntiforgeryTokenDto>(cancellationToken);

        return !string.IsNullOrWhiteSpace(token?.RequestToken)
            ? token.RequestToken
            : throw new InvalidOperationException(
                "The BFF returned an invalid antiforgery token response.");
    }

    private static void EnsureAuthenticated(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("The Blazor session is no longer valid.");
    }

    private static bool RequiresAntiforgery(HttpMethod method) =>
        method != HttpMethod.Get &&
        method != HttpMethod.Head &&
        method != HttpMethod.Options &&
        method != HttpMethod.Trace;

    private static string GetProxyUri(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint) ||
            endpoint.StartsWith('/') ||
            endpoint.StartsWith('\\') ||
            endpoint.Contains('\\') ||
            endpoint.Contains('#') ||
            Uri.TryCreate(endpoint, UriKind.Absolute, out _))
        {
            throw new ArgumentException(
                "A non-empty relative API path is required.",
                nameof(endpoint));
        }

        var path = endpoint.Split('?', 2)[0];
        if (Uri.UnescapeDataString(path)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException(
                "Relative API paths cannot contain traversal segments.",
                nameof(endpoint));
        }

        return $"api/proxy/{endpoint}";
    }
}
