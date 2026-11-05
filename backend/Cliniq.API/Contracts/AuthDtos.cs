using System.ComponentModel.DataAnnotations;

namespace Cliniq.API.Contracts;

public sealed class RegisterRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(72, MinimumLength = 12)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthResponse(UserResponseDto User, string AccessToken, DateTime ExpiresAtUtc);

public sealed class AppointmentCancellationRequest
{
    [Required, StringLength(500, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}

public sealed class InvoiceVoidRequest
{
    [Required, StringLength(500, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}

public sealed class RefundRequest
{
    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }
}

public sealed class SlotGenerationRequest
{
    [Required, MinLength(1)]
    public IReadOnlyCollection<DayOfWeek> DaysOfWeek { get; set; } = Array.Empty<DayOfWeek>();

    public TimeOnly StartTimeOfDayUtc { get; set; }
    public TimeOnly EndTimeOfDayUtc { get; set; }

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan SlotDuration { get; set; }
}

public sealed class EmptyRequest
{
}
