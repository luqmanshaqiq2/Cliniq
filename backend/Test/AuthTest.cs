using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Cliniq.API.Tests;

public sealed class AuthTest : IDisposable
{
    private const string ValidPassword = "correct-horse-battery";
    private readonly AuthApiFactory _factory = new();
    private readonly HttpClient _client;

    public AuthTest()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsPublicProfileWithoutSecrets()
    {
        const string password = "registration-secret-123";
        using var response = await PostJsonAsync("/api/auth/register", new
        {
            fullName = "Test Patient",
            email = "register-public@example.com",
            password
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(password, body);
        AssertNoSensitiveFields(body);

        using var document = JsonDocument.Parse(body);
        var user = document.RootElement;
        Assert.Equal("Test Patient", user.GetProperty("fullName").GetString());
        Assert.Equal("register-public@example.com", user.GetProperty("email").GetString());
        Assert.Equal("Patient", user.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Register_RejectsDuplicateEmailRegardlessOfCase()
    {
        using var firstResponse = await RegisterAsync(
            "duplicate-email@example.com",
            "first-account-secret");
        using var duplicateResponse = await RegisterAsync(
            "DUPLICATE-EMAIL@example.com",
            "second-account-secret");

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var body = await duplicateResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("first-account-secret", body);
        Assert.DoesNotContain("second-account-secret", body);
        AssertNoSensitiveFields(body);
    }

    [Theory]
    [InlineData("", "empty-name@example.com", "long-enough-password")]
    [InlineData("Test Patient", "invalid-email", "long-enough-password")]
    [InlineData("Test Patient", "short-password@example.com", "short")]
    [InlineData(null, "null-name@example.com", "long-enough-password")]
    [InlineData("Test Patient", null, "long-enough-password")]
    [InlineData("Test Patient", "null-password@example.com", null)]
    public async Task Register_RejectsInvalidFields(
        string? fullName,
        string? email,
        string? password)
    {
        using var response = await PostJsonAsync("/api/auth/register", new
        {
            fullName,
            email,
            password
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertNoSensitiveFields(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_RejectsPasswordsOverTheSupportedLength()
    {
        using var characterLimitResponse = await RegisterAsync(
            "long-password@example.com",
            new string('a', 73));
        using var utf8ByteLimitResponse = await RegisterAsync(
            "long-utf8-password@example.com",
            new string('é', 37));

        Assert.Equal(HttpStatusCode.BadRequest, characterLimitResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, utf8ByteLimitResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsTheSameGenericErrorForWrongPasswordAndUnknownEmail()
    {
        const string email = "login-user@example.com";
        using var registration = await RegisterAsync(email, ValidPassword);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        using var wrongPassword = await PostJsonAsync("/api/auth/login", new
        {
            email,
            password = "incorrect-password"
        });
        using var unknownEmail = await PostJsonAsync("/api/auth/login", new
        {
            email = "unknown-user@example.com",
            password = ValidPassword
        });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        Assert.Equal(
            await wrongPassword.Content.ReadAsStringAsync(),
            await unknownEmail.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_ReturnsNoPasswordOrHash()
    {
        const string email = "login-public@example.com";
        using var registration = await RegisterAsync(email, ValidPassword);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        using var response = await PostJsonAsync("/api/auth/login", new
        {
            email,
            password = ValidPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(ValidPassword, body);
        AssertNoSensitiveFields(body);
    }

    [Theory]
    [InlineData(null, "some-password")]
    [InlineData("", "some-password")]
    [InlineData("valid-login@example.com", null)]
    [InlineData("valid-login@example.com", "")]
    public async Task Login_RejectsNullOrEmptyCredentials(string? email, string? password)
    {
        using var response = await PostJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("' OR 1=1 --")]
    [InlineData("<script>alert(1)</script>")]
    public async Task Login_TreatsInjectionStringsAsCredentialData(string input)
    {
        using var invalidEmailResponse = await PostJsonAsync("/api/auth/login", new
        {
            email = input,
            password = input
        });
        using var injectionPasswordResponse = await PostJsonAsync("/api/auth/login", new
        {
            email = "injection-test@example.com",
            password = input
        });

        Assert.Equal(HttpStatusCode.BadRequest, invalidEmailResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, injectionPasswordResponse.StatusCode);
    }

    [Fact]
    public async Task Register_StoresScriptAndSqlStringsAsOrdinaryData()
    {
        const string name = "<script>alert(1)</script> ' OR 1=1 --";
        const string email = "injection-register@example.com";
        const string password = "valid-pass' OR '1'='1";
        using var response = await PostJsonAsync("/api/auth/register", new
        {
            fullName = name,
            email,
            password
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain(password, body);
        using var document = JsonDocument.Parse(body);
        Assert.Equal(
            name,
            document.RootElement.GetProperty("fullName").GetString());

        using var loginResponse = await PostJsonAsync("/api/auth/login", new
        {
            email,
            password
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ExceedsRequestRateLimitAfterConfiguredPerIpAllowance()
    {
        var rateLimited = false;
        for (var attempt = 0; attempt < 121; attempt++)
        {
            using var response = await PostJsonAsync("/api/auth/login", new
            {
                email = "rate-limit-user@example.com",
                password = "incorrect-password"
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Assert.Equal("60", response.Headers.GetValues("Retry-After").Single());
                rateLimited = true;
                break;
            }

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        Assert.True(rateLimited, "The API did not apply its configured request rate limit.");
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<HttpResponseMessage> RegisterAsync(string email, string password)
    {
        return await PostJsonAsync("/api/auth/register", new
        {
            fullName = "Test Patient",
            email,
            password
        });
    }

    private Task<HttpResponseMessage> PostJsonAsync(string path, object payload)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");
        return _client.PostAsync(path, content);
    }

    private static void AssertNoSensitiveFields(string body)
    {
        using var document = JsonDocument.Parse(body);
        AssertNoSensitiveFields(document.RootElement);
    }

    private static void AssertNoSensitiveFields(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var name = property.Name.ToLowerInvariant();
                Assert.DoesNotContain(
                    name,
                    new[] { "passwordhash", "stacktrace", "exception" });
                AssertNoSensitiveFields(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                AssertNoSensitiveFields(item);
            }
        }
    }

    private sealed class AuthApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(
            Path.GetTempPath(),
            $"cliniq-auth-tests-{Guid.NewGuid():N}.db");
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
        private readonly string _contentRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "Cliniq.API"));

        public AuthApiFactory()
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
            builder.UseContentRoot(_contentRoot);
            builder.UseEnvironment(Environments.Development);
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
