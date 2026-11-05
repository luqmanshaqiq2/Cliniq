using Backend.Cliniq.Models.Entities;
using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Interfaces;
using Cliniq.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.Services.DataServices.Implementation;

public sealed class DoctorDataService
    : BaseDataService.BaseDalDataService<Doctor, DoctorDataService>,
      IDoctorDataService
{
    public DoctorDataService(IDoctorRepo repository, IAppLogging<DoctorDataService> logger)
        : base(repository, logger)
    {
    }

    public async Task<IReadOnlyList<Doctor>> GetAvailableBySpecialtyAsync(
        string specialty,
        DateTime dateTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(specialty);
        var context = _mainRepo.Context;

        return await context.Doctors
            .AsNoTracking()
            .Where(doctor =>
                doctor.Specialization.ToLower() == specialty.Trim().ToLower() &&
                context.AvailabilitySlots.Any(slot =>
                    slot.DoctorId == doctor.Id &&
                    slot.IsAvailable &&
                    slot.StartAtUtc <= dateTime &&
                    dateTime < slot.EndAtUtc &&
                    !context.Appointments.Any(appointment =>
                        appointment.DoctorId == doctor.Id &&
                        appointment.Status != AppointmentStatus.Cancelled &&
                        appointment.ScheduledAtUtc < slot.EndAtUtc &&
                        slot.StartAtUtc < appointment.ScheduledEndAtUtc)))
            .OrderBy(doctor => doctor.FullName)
            .ToListAsync();
    }

    public Task<IReadOnlyList<Appointment>> SetLeaveAsync(
        int doctorId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        ValidateRange(fromUtc, toUtc);
        throw new NotSupportedException(
            "Doctor leave cannot be persisted because the current model has no leave or unavailability record.");
    }

    public Task<bool> HasActiveAppointmentsAsync(int doctorId)
    {
        var now = DateTime.UtcNow;
        return _mainRepo.Context.Appointments.AnyAsync(appointment =>
            appointment.DoctorId == doctorId &&
            (appointment.Status == AppointmentStatus.Scheduled ||
             appointment.Status == AppointmentStatus.Confirmed) &&
            appointment.ScheduledEndAtUtc > now);
    }

    public Task<bool> DeactivateAsync(int doctorId)
    {
        throw new NotSupportedException(
            "Doctor deactivation cannot be persisted because the doctor model has no active-status field.");
    }

    public Task<bool> ExistsByLicenseNumberAsync(string licenseNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(licenseNumber);
        throw new NotSupportedException(
            "License-number checks are unavailable because the doctor model has no license-number field.");
    }

    private static void ValidateRange(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            throw new ArgumentException("The end time must be later than the start time.");
        }
    }
}
