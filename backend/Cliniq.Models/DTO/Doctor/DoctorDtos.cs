using System.ComponentModel.DataAnnotations;

namespace Backend.Cliniq.Models.DTO.Doctor;

public sealed class DoctorCreateDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 1)]
    public string Specialization { get; set; } = string.Empty;
    [Required, StringLength(50, MinimumLength = 1)]
    public string ContactNumber { get; set; } = string.Empty;
    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }
}

public sealed class DoctorUpdateDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 1)]
    public string Specialization { get; set; } = string.Empty;
    [Required, StringLength(50, MinimumLength = 1)]
    public string ContactNumber { get; set; } = string.Empty;
    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }
}

public sealed class DoctorResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
