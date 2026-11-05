using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities.Base
{
    [EntityTypeConfiguration(typeof(UserConfiguration))]
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Patient;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }
    }

    public enum UserRole
    {
        Administrator = 1,
        Doctor = 2,
        Patient = 3,
        Staff = 4
    }
}