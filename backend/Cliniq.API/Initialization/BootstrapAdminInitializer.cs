using System.Text;
using Cliniq.DataAccess.EFStructure;
using Cliniq.Services.DataServices.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.API.Initialization;

public static class BootstrapAdminInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];
        var fullName = configuration["BootstrapAdmin:FullName"];
        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(password) &&
            string.IsNullOrWhiteSpace(fullName))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fullName) ||
            password.Length < 12 ||
            Encoding.UTF8.GetByteCount(password) > 72)
        {
            throw new InvalidOperationException(
                "BootstrapAdmin requires FullName, Email, and a password between 12 and 72 UTF-8 bytes.");
        }

        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await context.Users.AnyAsync(user => user.Role == UserRole.Administrator))
        {
            return;
        }

        if (await context.Users.AnyAsync(user => user.Email.ToLower() == email.Trim().ToLower()))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin email already belongs to a non-administrator account.");
        }

        context.Users.Add(new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            PasswordHash = JwtHelpers.HashPassword(password),
            Role = UserRole.Administrator,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }
}
