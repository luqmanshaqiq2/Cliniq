using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Interfaces;
using Cliniq.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.Services.DataServices.Implementation;

public sealed class UserDataService
    : BaseDataService.BaseDalDataService<User, UserDataService>,
      IUserDataService
{
    public UserDataService(IUserRepo repository, IAppLogging<UserDataService> logger)
        : base(repository, logger)
    {
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedEmail = email.Trim().ToLower();
        return await _mainRepo.Context.Users.AnyAsync(user =>
            user.Email.ToLower() == normalizedEmail);
    }

    public Task<User?> FindByEmailAsync(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedEmail = email.Trim().ToLower();
        return _mainRepo.Context.Users.SingleOrDefaultAsync(user =>
            user.Email.ToLower() == normalizedEmail);
    }

    public Task<bool> ExistsByUsernameAsync(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        throw new NotSupportedException(
            "Username checks are unavailable because the user model has no username field.");
    }

    public Task RecordFailedLoginAsync(int userId)
    {
        throw new NotSupportedException(
            "Failed-login tracking is unavailable because the user model has no lockout counters.");
    }

    public Task<bool> IsLockedOutAsync(int userId)
    {
        throw new NotSupportedException(
            "Lockout checks are unavailable because the user model has no lockout state.");
    }

    public Task ResetLockoutAsync(int userId)
    {
        throw new NotSupportedException(
            "Lockout reset is unavailable because the user model has no lockout state.");
    }

    public Task StoreRefreshTokenAsync(int userId, string refreshToken, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        throw new NotSupportedException(
            "Refresh tokens cannot be persisted because the current user model has no refresh-token storage.");
    }

    public async Task RevokeAllRefreshTokensAsync(int userId)
    {
        if (!await _mainRepo.Context.Users.AnyAsync(user => user.Id == userId))
        {
            throw new KeyNotFoundException($"User {userId} was not found.");
        }

        throw new NotSupportedException(
            "Refresh tokens cannot be revoked because the current model has no refresh-token storage.");
    }

    public async Task<bool> DeactivateAsync(int userId)
    {
        var user = await _mainRepo.Context.Users.SingleOrDefaultAsync(item => item.Id == userId);
        if (user is null)
        {
            return false;
        }

        user.IsActive = false;
        await _mainRepo.Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeRoleAsync(int userId, UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        var user = await _mainRepo.Context.Users.SingleOrDefaultAsync(item => item.Id == userId);
        if (user is null)
        {
            return false;
        }

        user.Role = role;
        await _mainRepo.Context.SaveChangesAsync();
        return true;
    }

    public Task<bool> LinkToDoctorOrPatientAsync(int userId, int? doctorId, int? patientId)
    {
        if (doctorId.HasValue && patientId.HasValue)
        {
            throw new ArgumentException("A user cannot be linked to both a doctor and a patient.");
        }

        if (!doctorId.HasValue && !patientId.HasValue)
        {
            throw new ArgumentException("A doctor ID or patient ID is required.");
        }

        throw new NotSupportedException(
            "User-to-doctor/patient links cannot be persisted because the user model has no relationship fields.");
    }
}
