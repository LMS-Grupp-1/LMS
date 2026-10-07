using LMS.Blazor.Client.Services;
using LMS.Blazor.Client.Services.ApiProxy;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace LMS.Blazor.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthenticationStateDeserialization();
        builder.Services.AddScoped<ICourseApiService, CourseApiService>();
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7003/")
        });
        builder.Services.AddScoped<IApiProxyClient, ApiProxyClient>();
        builder.Services.AddScoped<IUsersService, UsersService>();

        await builder.Build().RunAsync();
    }
}
