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
                var feature = context.Features.Get<IExceptionHandlerFeature>();

                if (feature is null)
                    return;
      
                    var exception = feature.Error;

                    var (statusCode, title, code) = exception switch
                    {
                        BadRequestException ex => (StatusCodes.Status400BadRequest, "Bad Request", ex.Code),
                        NotFoundException ex => (StatusCodes.Status404NotFound, ex.Title, ex.Code),
                        ConflictException ex => (StatusCodes.Status409Conflict, "Conflict", ex.Code),
                        TokenValidationException ex => (StatusCodes.Status401Unauthorized, "Unauthorized", "Unauthorized"),
                        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "InternalServerError")
                    };

                var factory = context.RequestServices
                    .GetRequiredService<ProblemDetailsFactory>();

                    var problem = factory.CreateProblemDetails(
                        context,
                        statusCode: statusCode,
                        title: title,
                        detail: statusCode == StatusCodes.Status500InternalServerError ?
                        "An unexpected error occured." : exception.Message,
                        instance: context.Request.Path);

                problem.Extensions["code"] = code;

                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsJsonAsync(problem);
                }

            });
    }
}
