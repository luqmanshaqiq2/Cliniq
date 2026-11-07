using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Cliniq.API.Tests;

public sealed class PatientWorkflowTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly ApiTestFactory _factory = new();

    public PatientWorkflowTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreatePatient_AndSearch_AndDuplicateDetection_ReturnExpectedResults()
    {
        var token = await RegisterAndLoginAsync("patient-lookup");
        var unique = Guid.NewGuid().ToString("N");
        var name = $"Taylor {unique[..8]}";
        var phone = "+15550000333";
        var dateOfBirth = new DateTime(1994, 5, 18, 0, 0, 0, DateTimeKind.Utc);

        using var createResponse = await SendAsync(
            HttpMethod.Post,
            "/api/patients",
            new
            {
                fullName = name,
                dateOfBirth,
                contactNumber = phone,
                email = $"{unique}@example.com",
                address = "121 Pine Street"
            },
            token);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var patientDocument = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var patientId = patientDocument.RootElement.GetProperty("id").GetInt32();

        using var searchResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/patients/search?term={Uri.EscapeDataString(unique[..12])}&pageSize=10&pageNumber=1",
            null,
            token);

        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        using var searchDocument = JsonDocument.Parse(await searchResponse.Content.ReadAsStringAsync());
        Assert.Contains(
            searchDocument.RootElement.EnumerateArray(),
            item => item.GetProperty("id").GetInt32() == patientId);

        using var duplicatesResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/patients/possible-duplicates?name={Uri.EscapeDataString(name)}&dateOfBirth={Uri.EscapeDataString(dateOfBirth.ToString("O"))}&phone={Uri.EscapeDataString(phone)}",
            null,
            token);

        Assert.Equal(HttpStatusCode.OK, duplicatesResponse.StatusCode);
        using var duplicatesDocument = JsonDocument.Parse(await duplicatesResponse.Content.ReadAsStringAsync());
        Assert.Contains(
            duplicatesDocument.RootElement.EnumerateArray(),
            item => item.GetProperty("id").GetInt32() == patientId);
    }

    [Fact]
    public async Task AnonymizePatient_RedactsPersonalInformation()
    {
        var token = await RegisterAndLoginAsync("patient-anonymizer");
        using var createResponse = await SendAsync(
            HttpMethod.Post,
            "/api/patients",
            new
            {
                fullName = "Mina Ora",
                dateOfBirth = new DateTime(1988, 2, 16, 0, 0, 0, DateTimeKind.Utc),
                contactNumber = "+15550009999",
                email = $"mina-{Guid.NewGuid():N}@example.com",
                address = "99 River Way"
            },
            token);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var createDocument = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var patientId = createDocument.RootElement.GetProperty("id").GetInt32();

        using var anonymizeResponse = await SendAsync(
            HttpMethod.Post,
            $"/api/patients/{patientId}/anonymize",
            null,
            token);

        Assert.Equal(HttpStatusCode.NoContent, anonymizeResponse.StatusCode);

        using var getResponse = await SendAsync(HttpMethod.Get, $"/api/patients/{patientId}", null, token);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        using var getDocument = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        Assert.Equal($"Anonymized patient {patientId}", getDocument.RootElement.GetProperty("fullName").GetString());
        Assert.Equal("REDACTED", getDocument.RootElement.GetProperty("contactNumber").GetString());
        Assert.Equal($"anonymized-{patientId}@invalid.local", getDocument.RootElement.GetProperty("email").GetString());
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
            $"cliniq-patient-tests-{Guid.NewGuid():N}.db");

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
