using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Cliniq.API.Tests;

public sealed class DoctorManagementTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly ApiTestFactory _factory = new();

    public DoctorManagementTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateDoctor_ThenGetById_AndAvailableBySpecialty_ReturnsTheSavedDoctor()
    {
        var token = await RegisterAndLoginAsync("doctor-owner");
        var doctorPayload = new
        {
            fullName = "Dr. Ada Rivers",
            specialization = "Cardiology",
            contactNumber = "+15550001111",
            email = $"ada-{Guid.NewGuid():N}@example.com"
        };

        using var createResponse = await SendAsync(
            HttpMethod.Post,
            "/api/doctors",
            doctorPayload,
            token);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var createDocument = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var doctorId = createDocument.RootElement.GetProperty("id").GetInt32();

        var slotAt = DateTime.UtcNow.AddDays(1).Date.AddHours(9);
        using var slotResponse = await SendAsync(
            HttpMethod.Post,
            "/api/AvailabilitySlots",
            new
            {
                doctorId,
                startAtUtc = slotAt,
                endAtUtc = slotAt.AddMinutes(30)
            },
            token);

        Assert.Equal(HttpStatusCode.Created, slotResponse.StatusCode);

        using var byIdResponse = await SendAsync(HttpMethod.Get, $"/api/doctors/{doctorId}", null, token);
        Assert.Equal(HttpStatusCode.OK, byIdResponse.StatusCode);

        var atUtc = slotAt.ToString("O");
        using var availableResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/doctors/available?specialty=Cardiology&atUtc={Uri.EscapeDataString(atUtc)}",
            null,
            token);

        Assert.Equal(HttpStatusCode.OK, availableResponse.StatusCode);
        using var availableDocument = JsonDocument.Parse(await availableResponse.Content.ReadAsStringAsync());
        Assert.Contains(
            availableDocument.RootElement.EnumerateArray(),
            item => item.GetProperty("id").GetInt32() == doctorId);
    }

    [Fact]
    public async Task GetUnknownDoctor_ReturnsNotFound()
    {
        var token = await RegisterAndLoginAsync("unknown-doctor-reader");

        using var response = await SendAsync(HttpMethod.Get, "/api/doctors/999999", null, token);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<string> RegisterAndLoginAsync(string baseName)
    {
        var email = $"{baseName}-{Guid.NewGuid():N}@example.com";
        var password = "correct-horse-battery";

        using var registerResponse = await SendAsync(
            HttpMethod.Post,
            "/api/auth/register",
            new { fullName = $"{baseName} User", email, password },
            null);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using var loginResponse = await SendAsync(
            HttpMethod.Post,
            "/api/auth/login",
            new { email, password },
            null);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        using var loginDocument = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        return loginDocument.RootElement.GetProperty("accessToken").GetString()!;
    }

    private Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? payload,
        string? bearerToken)
    {
        var request = new HttpRequestMessage(method, path);
        if (payload is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
        }

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return _client.SendAsync(request);
    }

    private sealed class ApiTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(
            Path.GetTempPath(),
            $"cliniq-doctor-tests-{Guid.NewGuid():N}.db");

        private readonly string[] _overriddenEnvironmentVariables =
        [
            "ASPNETCORE_ENVIRONMENT",
            "ConnectionStrings__DefaultConnection",
            "JwtOptions__Issuer",
            "JwtOptions__Audience",
            "JwtOptions__Lifetime",
            "JwtOptions__SignKey"
        ];

        private readonly Dictionary<string, string?> _originalEnvironmentValues;

        public ApiTestFactory()
        {
            _originalEnvironmentValues = _overriddenEnvironmentVariables.ToDictionary(
                name => name,
                Environment.GetEnvironmentVariable);

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Development);
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection",
                $"Data Source={_databasePath};Pooling=False");
            Environment.SetEnvironmentVariable("JwtOptions__Issuer", "Cliniq.API.Tests");
            Environment.SetEnvironmentVariable("JwtOptions__Audience", "Cliniq.Client.Tests");
            Environment.SetEnvironmentVariable("JwtOptions__Lifetime", "60");
            Environment.SetEnvironmentVariable(
                "JwtOptions__SignKey",
                "integration-test-only-signing-key-not-for-production");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Development);
            builder.UseContentRoot(Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "Cliniq.API")));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
            {
                return;
            }

            foreach (var (name, value) in _originalEnvironmentValues)
            {
                Environment.SetEnvironmentVariable(name, value);
            }

            foreach (var suffix in new[] { string.Empty, "-shm", "-wal" })
            {
                var path = _databasePath + suffix;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
