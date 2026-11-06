using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Cliniq.API.Middleware;
using Cliniq.API.Initialization;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Initialization;
using Cliniq.Models.Options;
using Cliniq.Services.DataServices.Implementation;
using Cliniq.Services.Logging;
using Cliniq.API.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "The 'DefaultConnection' connection string is not configured.");
var sqliteConnectionString = new SqliteConnectionStringBuilder(connectionString);
if (!Path.IsPathRooted(sqliteConnectionString.DataSource))
{
    sqliteConnectionString.DataSource = Path.GetFullPath(
        sqliteConnectionString.DataSource,
        builder.Environment.ContentRootPath);
}

var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>()
    ?? throw new InvalidOperationException("The 'JwtOptions' section is not configured.");
if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
    jwtOptions.Lifetime <= 0 ||
    string.IsNullOrWhiteSpace(jwtOptions.SignKey) ||
    Encoding.UTF8.GetByteCount(jwtOptions.SignKey) < 32)
{
    throw new InvalidOperationException(
        "JwtOptions must include an issuer, audience, positive lifetime, and a signing key of at least 32 bytes. " +
        "Configure the signing key with the JwtOptions__SignKey environment variable.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(sqliteConnectionString.ToString()));
builder.Services.AddTransient(typeof(IAppLogging<>), typeof(AppLogging<>));
builder.Services.AddDataServices();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSwaggerConfiguration(builder.Configuration);
builder.Services.AddApiRateLimiting();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SignKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

await SampleDataInitializer.InitializeAsync(app.Services);
await BootstrapAdminInitializer.InitializeAsync(app.Services, builder.Configuration);

app.UseMiddleware<ExceptionHandlingMiddleware>();
var staticFilesPath = Path.Combine(builder.Environment.ContentRootPath, "StaticFiles");
Directory.CreateDirectory(staticFilesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staticFilesPath),
    RequestPath = "/StaticFiles"
});
app.UseRouting();
app.UseApiRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Cliniq API v1"));
}

app.Run();
