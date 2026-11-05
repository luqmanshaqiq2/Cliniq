using Backend.Cliniq.Models.Entities;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface IAppointmentDataService : IBaseDataService<Appointment>
    {
        Task<bool> HasConflictAsync(
            int doctorId,
            DateTime start,
            DateTime end,
            int? excludeAppointmentId = null);

        Task<bool> HasPatientOverlapAsync(
            int patientId,
            DateTime start,
            DateTime end);

        Task<AppointmentBookingResult> TryBookAsync(
            int patientId,
            int slotId,
            string? notes = null);

        Task<AppointmentBookingResult> RescheduleAsync(int id, int newSlotId);

        Task<IReadOnlyList<Appointment>> GetByStatusAndRangeAsync(
            AppointmentStatus status,
            DateTime fromUtc,
            DateTime toUtc,
            int pageSize,
            int pageNumber);

        Task<AppointmentTransitionStatus> MarkNoShowAsync(int id);

        Task<AppointmentTransitionStatus> CancelAsync(
            int id,
            string reason,
            int cancelledBy);

        Task<IReadOnlyList<Appointment>> GetUpcomingForReminderAsync(TimeSpan window);
    }
}
