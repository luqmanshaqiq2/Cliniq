using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Cliniq.API.Tests;

public sealed class AppointmentWorkflowTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly ApiTestFactory _factory = new();

    public AppointmentWorkflowTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateAndRescheduleAppointment_UpdatesTheScheduledWindow()
    {
        var token = await RegisterAndLoginAsync("booking-user");
        var doctorId = await CreateDoctorAsync(token, "Dr. Priya West", "Neurology");
        var patientId = await CreatePatientAsync(token);

        var originalSlot = new
        {
            doctorId,
            startAtUtc = new DateTime(2030, 1, 15, 9, 0, 0, DateTimeKind.Utc),
            endAtUtc = new DateTime(2030, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };
        var replacementSlot = new
        {
            doctorId,
            startAtUtc = new DateTime(2030, 1, 15, 11, 0, 0, DateTimeKind.Utc),
            endAtUtc = new DateTime(2030, 1, 15, 12, 0, 0, DateTimeKind.Utc)
        };

        using var originalSlotResponse = await SendAsync(
            HttpMethod.Post,
            "/api/AvailabilitySlots",
            originalSlot,
            token);
        Assert.Equal(HttpStatusCode.Created, originalSlotResponse.StatusCode);
        using var replacementSlotResponse = await SendAsync(
            HttpMethod.Post,
            "/api/AvailabilitySlots",
            replacementSlot,
            token);
        Assert.Equal(HttpStatusCode.Created, replacementSlotResponse.StatusCode);

        using var originalSlotDocument = JsonDocument.Parse(await originalSlotResponse.Content.ReadAsStringAsync());
        var originalSlotId = originalSlotDocument.RootElement.GetProperty("id").GetInt32();
        using var replacementSlotDocument = JsonDocument.Parse(await replacementSlotResponse.Content.ReadAsStringAsync());
        var replacementSlotId = replacementSlotDocument.RootElement.GetProperty("id").GetInt32();

        using var appointmentResponse = await SendAsync(
            HttpMethod.Post,
            "/api/appointments",
            new
            {
                scheduledAtUtc = originalSlot.startAtUtc,
                scheduledEndAtUtc = originalSlot.endAtUtc,
                notes = "Initial consultation",
                patientId,
                doctorId
            },
            token);

        Assert.Equal(HttpStatusCode.Created, appointmentResponse.StatusCode);
        using var appointmentDocument = JsonDocument.Parse(await appointmentResponse.Content.ReadAsStringAsync());
        var appointmentId = appointmentDocument.RootElement.GetProperty("id").GetInt32();
        Assert.Equal("Scheduled", appointmentDocument.RootElement.GetProperty("status").GetString());

        using var rescheduleResponse = await SendAsync(
            HttpMethod.Post,
            $"/api/appointments/{appointmentId}/reschedule?newSlotId={replacementSlotId}",
            null,
            token);

        Assert.Equal(HttpStatusCode.OK, rescheduleResponse.StatusCode);
        using var rescheduledDocument = JsonDocument.Parse(await rescheduleResponse.Content.ReadAsStringAsync());

        var scheduledAt = rescheduledDocument.RootElement.GetProperty("scheduledAtUtc").GetString()!;
        var scheduledEndAt = rescheduledDocument.RootElement.GetProperty("scheduledEndAtUtc").GetString()!;

        Assert.Equal(
            replacementSlot.startAtUtc.ToString("yyyy-MM-ddTHH:mm:ss"),
            scheduledAt.TrimEnd('Z'));
        Assert.Equal(
            replacementSlot.endAtUtc.ToString("yyyy-MM-ddTHH:mm:ss"),
            scheduledEndAt.TrimEnd('Z'));

        using var cancellationResponse = await SendAsync(
            HttpMethod.Post,
            $"/api/appointments/{appointmentId}/cancel",
            new { reason = "Patient rescheduled by request" },
            token);

        Assert.Equal(HttpStatusCode.NoContent, cancellationResponse.StatusCode);
    }

    [Fact]
    public async Task BookingConflict_RejectsOverlappingAppointments()
    {
        var token = await RegisterAndLoginAsync("booking-conflict-user");
        var doctorId = await CreateDoctorAsync(token, "Dr. Lin Hsu", "Orthopedics");
        var patientId = await CreatePatientAsync(token);

        var slotTime = new DateTime(2031, 2, 5, 14, 0, 0, DateTimeKind.Utc);
        using var slotResponse = await SendAsync(
            HttpMethod.Post,
            "/api/AvailabilitySlots",
            new
            {
                doctorId,
                startAtUtc = slotTime,
                endAtUtc = slotTime.AddMinutes(30)
            },
            token);

        Assert.Equal(HttpStatusCode.Created, slotResponse.StatusCode);

        using var firstAppointmentResponse = await SendAsync(
            HttpMethod.Post,
            "/api/appointments",
            new
            {
                scheduledAtUtc = slotTime,
                scheduledEndAtUtc = slotTime.AddMinutes(30),
                patientId,
                doctorId,
                notes = "First visit"
            },
            token);
        Assert.Equal(HttpStatusCode.Created, firstAppointmentResponse.StatusCode);

        var secondPatientId = await CreatePatientAsync(token, "Second Patient");
        using var conflictResponse = await SendAsync(
            HttpMethod.Post,
            "/api/appointments",
            new
            {
                scheduledAtUtc = slotTime,
                scheduledEndAtUtc = slotTime.AddMinutes(30),
                patientId = secondPatientId,
                doctorId,
                notes = "Double-booking attempt"
            },
            token);

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<int> CreateDoctorAsync(string bearerToken, string fullName, string specialization)
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/doctors",
            new
            {
                fullName,
                specialization,
                contactNumber = "+15550002222",
                email = $"{Guid.NewGuid():N}@example.com"
            },
            bearerToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<int> CreatePatientAsync(string bearerToken, string fullName = "Sample Patient")
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/patients",
            new
            {
                fullName,
                dateOfBirth = new DateTime(1992, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                contactNumber = "+15550005555",
                email = $"{Guid.NewGuid():N}@example.com",
                address = "99 Maple Drive"
            },
            bearerToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetInt32();
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
            $"cliniq-appointment-tests-{Guid.NewGuid():N}.db");

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
