namespace LMS.Blazor.Services.Authentication;

public readonly record struct RemoteLoginResult(RemoteLoginStatus Status)
{
    public static RemoteLoginResult Succeeded => new(RemoteLoginStatus.Succeeded);
    public static RemoteLoginResult InvalidCredentials => new(RemoteLoginStatus.InvalidCredentials);
    public static RemoteLoginResult ServiceError => new(RemoteLoginStatus.ServiceError);
}
