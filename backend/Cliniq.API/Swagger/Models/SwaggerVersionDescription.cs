namespace Cliniq.API.Swagger.Models;

public sealed class SwaggerVersionDescription
{
    public string Name { get; set; } = "v1";
    public string Description { get; set; } = string.Empty;
    public bool IsDeprecated { get; set; }
}
