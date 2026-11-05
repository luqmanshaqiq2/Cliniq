using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Services.DataServices.Base;
using System;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface IUserDataService : IBaseDataService<User>
    {
        Task<bool> ExistsByEmailAsync(string email);

        Task<User?> FindByEmailAsync(string email);

        Task<bool> ExistsByUsernameAsync(string username);

        Task RecordFailedLoginAsync(int userId);

        Task<bool> IsLockedOutAsync(int userId);

        Task ResetLockoutAsync(int userId);

        Task StoreRefreshTokenAsync(
            int userId,
            string refreshToken,
            DateTime expiresAtUtc);

        Task RevokeAllRefreshTokensAsync(int userId);

        Task<bool> DeactivateAsync(int userId);

        Task<bool> ChangeRoleAsync(int userId, UserRole role);

        Task<bool> LinkToDoctorOrPatientAsync(
            int userId,
            int? doctorId,
            int? patientId);
    }
}
