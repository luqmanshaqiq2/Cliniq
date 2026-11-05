using System.ComponentModel.DataAnnotations;

namespace Backend.Cliniq.Models.DTO.Patient;

public sealed class PatientCreateDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    [Required, StringLength(50, MinimumLength = 1)]
    public string ContactNumber { get; set; } = string.Empty;
    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }
    [StringLength(1000)]
    public string? Address { get; set; }
}

public sealed class PatientUpdateDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    [Required, StringLength(50, MinimumLength = 1)]
    public string ContactNumber { get; set; } = string.Empty;
    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }
    [StringLength(1000)]
    public string? Address { get; set; }
}

public sealed class PatientResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}
