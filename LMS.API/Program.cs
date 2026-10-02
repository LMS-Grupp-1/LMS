using LMS.API.Extensions;
using LMS.API.Services;
using LMS.Infrastructure.Data;
using LMS.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContext") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

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
        builder.Services.ConfigureAuthentication(builder.Configuration);
        builder.Services.ConfigureIdentity();
        builder.Services.ConfigurePolicys();

        var app = builder.Build();

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

        app.Run();
    }
}