namespace LMS.Blazor.Client.Services.ApiProxy;

public static class AntiforgeryProtocol
{
    public const string HeaderName = "X-XSRF-TOKEN";
    public const string TokenEndpoint = "api/antiforgery/token";
}
