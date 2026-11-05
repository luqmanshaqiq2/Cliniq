using Cliniq.API.Swagger.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Cliniq.API.Swagger;

public sealed class SwaggerDocumentFilter : IOpenApiDocumentTransformer
{
    private readonly SwaggerApplicationSettings _settings;

    public SwaggerDocumentFilter(IOptions<SwaggerConfigOptions> options)
    {
        _settings = options.Value.Application;
    }

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = _settings.Title,
            Description = _settings.Description,
            Version = _settings.Version
        };
        return Task.CompletedTask;
    }
}
