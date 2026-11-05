using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

namespace Cliniq.API.Swagger;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SwaggerConfigOptions>(configuration.GetSection("Swagger"));
        services.AddTransient<SwaggerDocumentFilter>();
        services.AddOpenApi("v1", options =>
            options.AddDocumentTransformer<SwaggerDocumentFilter>());
        return services;
    }
}
