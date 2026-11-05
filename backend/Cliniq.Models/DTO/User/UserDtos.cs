using Backend.Cliniq.Models.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Backend.Cliniq.Models.DTO.User;

public sealed class UserCreateDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;
    [Required, StringLength(72, MinimumLength = 12)]
    public string Password { get; set; } = string.Empty;
    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; } = UserRole.Patient;
}

public sealed class UserUpdateDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;
    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}

public sealed  class UserResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
