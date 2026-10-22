using LMS.API.Extensions;
using LMS.API.Services;
using LMS.Infrastructure.Data;
using LMS.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContext") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found.");
		// Retries automatically on transient errors, which Azure SQL can have during maintenance.
		builder.Services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

		builder.Services.AddControllers(opt =>
        {
            opt.ReturnHttpNotAcceptable = true;
            opt.Filters.Add(new ProducesAttribute("application/json"));
        })
        .AddApplicationPart(typeof(AssemblyReference).Assembly);

        builder.Services.AddHostedService<DataSeedService>();
        builder.Services.ConfigureSwagger();
        builder.Services.AddRepositories();
        builder.Services.AddServiceLayer();
        builder.Services.AddDocumentStorage(builder.Configuration);
        builder.Services.ConfigureAuthentication(builder.Configuration);
        builder.Services.ConfigureIdentity();
        builder.Services.ConfigurePolicys();
		builder.Services.ConfigureCors(builder.Configuration);

        var app = builder.Build();

		// Local development only (checked inside the method): starts Azurite before
		// DataSeedService creates and clears the blob container. Does nothing in Azure.
		await app.EnsureAzuriteRunningAsync();

		app.ConfigureExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers()
            .RequireAuthorization("Default");

        await app.RunAsync();
    }
}