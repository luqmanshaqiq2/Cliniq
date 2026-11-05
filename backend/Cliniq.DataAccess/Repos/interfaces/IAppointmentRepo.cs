using Backend.Cliniq.Models.Entities;
using Cliniq.DataAccess.Repos.Base;

namespace Cliniq.DataAccess.Repos.interfaces
{
    public interface IAppointmentRepo : IBaseRepo<Appointment>
    {
        Task<bool> HasScheduleConflictAsync(
            int doctorId,
            int patientId,
            DateTime startAtUtc,
            DateTime endAtUtc,
            int? excludeAppointmentId = null);

        Task<IReadOnlyList<Appointment>> GetForDoctorBetweenAsync(
            int doctorId,
            DateTime startAtUtc,
            DateTime endAtUtc);
    }
}
