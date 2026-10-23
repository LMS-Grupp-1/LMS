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
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        });
        builder.Services.AddScoped<IApiProxyClient, ApiProxyClient>();
		builder.Services.AddScoped<ICourseService, CourseService>();
		builder.Services.AddScoped<IAssignmentService, AssignmentService>();
		builder.Services.AddScoped<ISubmissionService, SubmissionService>();
		builder.Services.AddScoped<IAssignmentService, AssignmentService>();		

		builder.Services.AddScoped<ToastService>();
        builder.Services.AddScoped<DocumentService>();

		await builder.Build().RunAsync();
    }
}
