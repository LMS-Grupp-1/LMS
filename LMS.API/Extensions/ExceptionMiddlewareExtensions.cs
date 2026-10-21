using Domain.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LMS.API.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            AllowStatusCode404Response = true,

            ExceptionHandler = async context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                if (contextFeature is null)
                    return;
      
                var exception = contextFeature.Error;

                var (statusCode, title) = exception switch
                {
                    BadRequestException ex => (StatusCodes.Status400BadRequest, "Bad Request"),
                    NotFoundException ex => (StatusCodes.Status404NotFound, ex.Title),
                    ConflictException ex => (StatusCodes.Status409Conflict, "Conflict"),
                    TokenValidationException ex => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                    KeyNotFoundException ex =>  (StatusCodes.Status404NotFound, "Key not found"),
                    InvalidOperationException ex => (StatusCodes.Status400BadRequest, "Invalid Operation"),
                    DocumentAccessException ex => (StatusCodes.Status403Forbidden, "You don't have permission to access this resource"),
                    DocumentValidationException ex => (StatusCodes.Status400BadRequest, "Bad Request"),
                        
                    _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
                };

                var problemDetailsFactory = context.RequestServices
                    .GetRequiredService<ProblemDetailsFactory>();

                var problemDetails = problemDetailsFactory.CreateProblemDetails(
                    context,
                    statusCode: statusCode,
                    title: title,
                    detail: exception.Message,
                    instance: context.Request.Path);

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }

        });
    }
}