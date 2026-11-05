using Backend.Cliniq.Models.Entities;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Repos.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.DataAccess.Repos
{
    public class AppointmentRepo : BaseRepo<Appointment>, IAppointmentRepo
    {
        public AppointmentRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> HasScheduleConflictAsync(
            int doctorId,
            int patientId,
            DateTime startAtUtc,
            DateTime endAtUtc,
            int? excludeAppointmentId = null)
        {
            ValidateRange(startAtUtc, endAtUtc);

            return await Table.AsNoTracking().AnyAsync(appointment =>
                appointment.Status != AppointmentStatus.Cancelled &&
                (!excludeAppointmentId.HasValue || appointment.Id != excludeAppointmentId.Value) &&
                (appointment.DoctorId == doctorId || appointment.PatientId == patientId) &&
                appointment.ScheduledAtUtc < endAtUtc &&
                startAtUtc < appointment.ScheduledEndAtUtc);
        }

        public async Task<IReadOnlyList<Appointment>> GetForDoctorBetweenAsync(
            int doctorId,
            DateTime startAtUtc,
            DateTime endAtUtc)
        {
            ValidateRange(startAtUtc, endAtUtc);

            return await Table.AsNoTracking()
                .Where(appointment =>
                    appointment.DoctorId == doctorId &&
                    appointment.ScheduledAtUtc < endAtUtc &&
                    startAtUtc < appointment.ScheduledEndAtUtc)
                .OrderBy(appointment => appointment.ScheduledAtUtc)
                .ToListAsync();
        }

        private static void ValidateRange(DateTime startAtUtc, DateTime endAtUtc)
        {
            if (startAtUtc >= endAtUtc)
            {
                throw new ArgumentException("The end time must be later than the start time.");
            }
        }
    }
}
