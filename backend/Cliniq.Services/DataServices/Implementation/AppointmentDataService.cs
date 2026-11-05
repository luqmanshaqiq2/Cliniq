using Backend.Cliniq.Models.Entities;
using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Contracts;
using Cliniq.Services.DataServices.Interfaces;
using Cliniq.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.Services.DataServices.Implementation;

public sealed class AppointmentDataService
    : BaseDataService.BaseDalDataService<Appointment, AppointmentDataService>,
      IAppointmentDataService
{
    public AppointmentDataService(
        IAppointmentRepo repository,
        IAppLogging<AppointmentDataService> logger)
        : base(repository, logger)
    {
    }

    public async Task<bool> HasConflictAsync(
        int doctorId,
        DateTime start,
        DateTime end,
        int? excludeAppointmentId = null)
    {
        ValidateRange(start, end);

        return await _mainRepo.Context.Appointments.AnyAsync(appointment =>
            appointment.DoctorId == doctorId &&
            appointment.Status != AppointmentStatus.Cancelled &&
            (!excludeAppointmentId.HasValue || appointment.Id != excludeAppointmentId.Value) &&
            appointment.ScheduledAtUtc < end &&
            start < appointment.ScheduledEndAtUtc);
    }

    public async Task<bool> HasPatientOverlapAsync(int patientId, DateTime start, DateTime end)
    {
        ValidateRange(start, end);

        return await _mainRepo.Context.Appointments.AnyAsync(appointment =>
            appointment.PatientId == patientId &&
            appointment.Status != AppointmentStatus.Cancelled &&
            appointment.ScheduledAtUtc < end &&
            start < appointment.ScheduledEndAtUtc);
    }

    public async Task<AppointmentBookingResult> TryBookAsync(
        int patientId,
        int slotId,
        string? notes = null)
    {
        var context = _mainRepo.Context;
        var slot = await context.AvailabilitySlots
            .SingleOrDefaultAsync(candidate => candidate.Id == slotId);

        if (slot is null || !slot.IsAvailable)
        {
            return new AppointmentBookingResult(AppointmentBookingStatus.SlotTaken);
        }

        if (!await context.Doctors.AnyAsync(doctor => doctor.Id == slot.DoctorId))
        {
            return new AppointmentBookingResult(AppointmentBookingStatus.DoctorUnavailable);
        }

        if (!await context.Patients.AnyAsync(patient => patient.Id == patientId))
        {
            throw new KeyNotFoundException($"Patient {patientId} was not found.");
        }

        var conflicts = await context.Appointments.AnyAsync(appointment =>
            appointment.Status != AppointmentStatus.Cancelled &&
            (appointment.DoctorId == slot.DoctorId || appointment.PatientId == patientId) &&
            appointment.ScheduledAtUtc < slot.EndAtUtc &&
            slot.StartAtUtc < appointment.ScheduledEndAtUtc);

        if (conflicts)
        {
            var doctorConflict = await HasConflictAsync(
                slot.DoctorId, slot.StartAtUtc, slot.EndAtUtc);
            return new AppointmentBookingResult(
                doctorConflict
                    ? AppointmentBookingStatus.DoctorUnavailable
                    : AppointmentBookingStatus.PatientOverlap);
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        slot.IsAvailable = false;
        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = slot.DoctorId,
            ScheduledAtUtc = slot.StartAtUtc,
            ScheduledEndAtUtc = slot.EndAtUtc,
            Notes = notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        await _mainRepo.AddAsync(appointment);
        await transaction.CommitAsync();

        return new AppointmentBookingResult(AppointmentBookingStatus.Booked, appointment);
    }

    public async Task<AppointmentBookingResult> RescheduleAsync(int id, int newSlotId)
    {
        var context = _mainRepo.Context;
        var appointment = await context.Appointments
            .SingleOrDefaultAsync(item => item.Id == id);
        if (appointment is null)
        {
            return new AppointmentBookingResult(AppointmentBookingStatus.AppointmentNotFound);
        }

        if (appointment.Status is AppointmentStatus.Cancelled
            or AppointmentStatus.Completed
            or AppointmentStatus.NoShow)
        {
            return new AppointmentBookingResult(AppointmentBookingStatus.InvalidAppointmentState);
        }

        var newSlot = await context.AvailabilitySlots
            .SingleOrDefaultAsync(slot => slot.Id == newSlotId);
        var currentSlot = await context.AvailabilitySlots.SingleOrDefaultAsync(slot =>
            slot.DoctorId == appointment.DoctorId &&
            slot.StartAtUtc == appointment.ScheduledAtUtc &&
            slot.EndAtUtc == appointment.ScheduledEndAtUtc);

        if (newSlot is null)
        {
            return new AppointmentBookingResult(AppointmentBookingStatus.SlotTaken);
        }

        if (newSlot.Id != currentSlot?.Id && !newSlot.IsAvailable)
        {
            return new AppointmentBookingResult(AppointmentBookingStatus.SlotTaken);
        }

        if (newSlot.DoctorId != appointment.DoctorId)
        {
            return new AppointmentBookingResult(AppointmentBookingStatus.DoctorUnavailable);
        }

        var conflict = await context.Appointments.AnyAsync(other =>
            other.Id != id &&
            other.Status != AppointmentStatus.Cancelled &&
            (other.DoctorId == newSlot.DoctorId || other.PatientId == appointment.PatientId) &&
            other.ScheduledAtUtc < newSlot.EndAtUtc &&
            newSlot.StartAtUtc < other.ScheduledEndAtUtc);

        if (conflict)
        {
            var doctorConflict = await HasConflictAsync(
                newSlot.DoctorId, newSlot.StartAtUtc, newSlot.EndAtUtc, id);
            return new AppointmentBookingResult(
                doctorConflict
                    ? AppointmentBookingStatus.DoctorUnavailable
                    : AppointmentBookingStatus.PatientOverlap);
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        if (currentSlot is not null && currentSlot.Id != newSlot.Id)
        {
            currentSlot.IsAvailable = true;
        }

        newSlot.IsAvailable = false;
        appointment.ScheduledAtUtc = newSlot.StartAtUtc;
        appointment.ScheduledEndAtUtc = newSlot.EndAtUtc;
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new AppointmentBookingResult(AppointmentBookingStatus.Booked, appointment);
    }

    public async Task<IReadOnlyList<Appointment>> GetByStatusAndRangeAsync(
        AppointmentStatus status,
        DateTime fromUtc,
        DateTime toUtc,
        int pageSize,
        int pageNumber)
    {
        ValidateRange(fromUtc, toUtc);
        ValidatePage(pageSize, pageNumber);

        return await _mainRepo.Context.Appointments
            .AsNoTracking()
            .Where(appointment =>
                appointment.Status == status &&
                appointment.ScheduledAtUtc >= fromUtc &&
                appointment.ScheduledAtUtc < toUtc)
            .OrderBy(appointment => appointment.ScheduledAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<AppointmentTransitionStatus> MarkNoShowAsync(int id)
    {
        var appointment = await _mainRepo.Context.Appointments
            .SingleOrDefaultAsync(item => item.Id == id);
        if (appointment is null)
        {
            return AppointmentTransitionStatus.AppointmentNotFound;
        }

        if (appointment.Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
        {
            return AppointmentTransitionStatus.InvalidState;
        }

        appointment.Status = AppointmentStatus.NoShow;
        await _mainRepo.Context.SaveChangesAsync();
        return AppointmentTransitionStatus.Updated;
    }

    public async Task<AppointmentTransitionStatus> CancelAsync(
        int id,
        string reason,
        int cancelledBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (cancelledBy <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cancelledBy));
        }

        var context = _mainRepo.Context;
        var appointment = await context.Appointments.SingleOrDefaultAsync(item => item.Id == id);
        if (appointment is null)
        {
            return AppointmentTransitionStatus.AppointmentNotFound;
        }

        if (appointment.Status is AppointmentStatus.Cancelled
            or AppointmentStatus.Completed
            or AppointmentStatus.NoShow)
        {
            return AppointmentTransitionStatus.InvalidState;
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.Notes = string.IsNullOrWhiteSpace(appointment.Notes)
            ? $"Cancelled by user {cancelledBy}: {reason}"
            : $"{appointment.Notes}{Environment.NewLine}Cancelled by user {cancelledBy}: {reason}";

        var slot = await context.AvailabilitySlots.SingleOrDefaultAsync(candidate =>
            candidate.DoctorId == appointment.DoctorId &&
            candidate.StartAtUtc == appointment.ScheduledAtUtc &&
            candidate.EndAtUtc == appointment.ScheduledEndAtUtc);
        if (slot is not null)
        {
            var otherActiveAppointment = await context.Appointments.AnyAsync(other =>
                other.Id != appointment.Id &&
                other.DoctorId == appointment.DoctorId &&
                other.Status != AppointmentStatus.Cancelled &&
                other.ScheduledAtUtc < slot.EndAtUtc &&
                slot.StartAtUtc < other.ScheduledEndAtUtc);
            if (!otherActiveAppointment)
            {
                slot.IsAvailable = true;
            }
        }

        await context.SaveChangesAsync();
        return AppointmentTransitionStatus.Updated;
    }

    public async Task<IReadOnlyList<Appointment>> GetUpcomingForReminderAsync(TimeSpan window)
    {
        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window));
        }

        var now = DateTime.UtcNow;
        return await _mainRepo.Context.Appointments
            .AsNoTracking()
            .Where(appointment =>
                (appointment.Status == AppointmentStatus.Scheduled ||
                 appointment.Status == AppointmentStatus.Confirmed) &&
                appointment.ScheduledAtUtc >= now &&
                appointment.ScheduledAtUtc < now + window)
            .OrderBy(appointment => appointment.ScheduledAtUtc)
            .ToListAsync();
    }

    private static void ValidateRange(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            throw new ArgumentException("The end time must be later than the start time.");
        }
    }

    private static void ValidatePage(int pageSize, int pageNumber)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }
    }
}
