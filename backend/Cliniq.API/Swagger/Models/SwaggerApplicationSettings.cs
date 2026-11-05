namespace Cliniq.API.Swagger.Models;

public sealed class SwaggerApplicationSettings
{
    public string Title { get; set; } = "Cliniq API";
    public string Description { get; set; } = "HTTP API for Cliniq clinic services.";
    public string Version { get; set; } = "v1";
}
