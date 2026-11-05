using Backend.Cliniq.Models.Entities.Base;
using Backend.Cliniq.Models.Entities;
using Cliniq.Services.DataServices.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface IDoctorDataService : IBaseDataService<Doctor>
    {
        Task<IReadOnlyList<Doctor>> GetAvailableBySpecialtyAsync(
            string specialty,
            DateTime dateTime);

        Task<IReadOnlyList<Appointment>> SetLeaveAsync(
            int doctorId,
            DateTime fromUtc,
            DateTime toUtc);

        Task<bool> HasActiveAppointmentsAsync(int doctorId);

        Task<bool> DeactivateAsync(int doctorId);

        Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
    }
}
