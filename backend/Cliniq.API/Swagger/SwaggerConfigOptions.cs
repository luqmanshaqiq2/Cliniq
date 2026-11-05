using Cliniq.API.Swagger.Models;

namespace Cliniq.API.Swagger;

public sealed class SwaggerConfigOptions
{
    public SwaggerApplicationSettings Application { get; set; } = new();
}
