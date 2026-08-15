using Microsoft.OpenApi;

namespace Rivo.API.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddRivoSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Rivo API", Version = "v1" });

            var jwtScheme = new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Description = "Введите JWT токен в формате: Bearer {token}",
            };

            options.AddSecurityDefinition("Bearer", jwtScheme);

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", null), new List<string>() },
            });
        });

        return services;
    }
}
