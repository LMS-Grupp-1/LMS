using Domain.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LMS.API.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(builder =>
        {
            
            builder.Run(async context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var problemDetailsFactory = app.Services.GetRequiredService<ProblemDetailsFactory>();
                    var exception = contextFeature.Error;

                    var (statusCode, title) = exception switch
                    {
                        NotFoundException ex => (StatusCodes.Status404NotFound, ex.Title),
                        TokenValidationException ex => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                        KeyNotFoundException ex =>  (StatusCodes.Status404NotFound, "Key not found"),
                        InvalidOperationException ex => (StatusCodes.Status400BadRequest, "Invalid Operation"),
                        DocumentAccessException ex => (StatusCodes.Status403Forbidden, "You don't have permission to access this resource"),
                        DocumentValidationException ex => (StatusCodes.Status400BadRequest, "Bad Request"),
                        
                        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
                    };

                    var problemDetails = problemDetailsFactory.CreateProblemDetails(
                        context,
                        statusCode,
                        title,
                        detail: exception.Message,
                        instance: context.Request.Path);

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(problemDetails);
                }

            });
        });
    }
}
