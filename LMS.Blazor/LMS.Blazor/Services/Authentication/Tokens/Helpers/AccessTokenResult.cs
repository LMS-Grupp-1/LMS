namespace LMS.Blazor.Services.Authentication.Tokens.Helpers;

public readonly record struct AccessTokenResult(AccessTokenStatus Status, string? AccessToken)
{
    public static AccessTokenResult Success(string accessToken) =>
        new(AccessTokenStatus.Success, accessToken);

    public static AccessTokenResult Unauthorized =>
        new(AccessTokenStatus.Unauthorized, null);

    public static AccessTokenResult Unavailable =>
        new(AccessTokenStatus.Unavailable, null);
}
