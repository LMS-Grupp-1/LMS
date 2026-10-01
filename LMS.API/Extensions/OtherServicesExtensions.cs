using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Filters;

namespace LMS.API.Extensions;

public static class OtherServicesExtensions
{
    public static void ConfigureSwagger(this IServiceCollection services) =>
               services.AddEndpointsApiExplorer()
              .AddSwaggerGen(setup =>
              {
                  setup.EnableAnnotations();

                  setup.SwaggerDoc("v1", new OpenApiInfo
                  {
                      Title = "LMS API",
                      Version = "v1"
                  });


                  setup.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                  {
                      In = ParameterLocation.Header,
                      Description = "Paste in JWT access token",
                      Name = "Authorization",
                      Type = SecuritySchemeType.Http,
                      BearerFormat = "JWT",
                      Scheme = "bearer"
                  });

                  setup.OperationFilter<SecurityRequirementsOperationFilter>(true, "bearer");

              });
}
