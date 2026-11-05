using Backend.Cliniq.Models.Entities;
using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Contracts;
using Cliniq.Services.DataServices.Interfaces;
using Cliniq.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.Services.DataServices.Implementation;

public sealed class AvailabilitySlotDataService
    : BaseDataService.BaseDalDataService<AvailabilitySlot, AvailabilitySlotDataService>,
      IAvailabilitySlotDataService
{
    public AvailabilitySlotDataService(
        IAvailabilitySlotRepo repository,
        IAppLogging<AvailabilitySlotDataService> logger)
        : base(repository, logger)
    {
    }

    public async Task<bool> ClaimSlotAsync(int slotId)
    {
        return await _mainRepo.Context.AvailabilitySlots
            .Where(slot => slot.Id == slotId && slot.IsAvailable)
            .ExecuteUpdateAsync(update => update.SetProperty(slot => slot.IsAvailable, false)) == 1;
    }

    public async Task<bool> ReleaseSlotAsync(int slotId)
    {
        var context = _mainRepo.Context;
        var slot = await context.AvailabilitySlots.SingleOrDefaultAsync(item => item.Id == slotId);
        if (slot is null)
        {
            return false;
        }

        var hasActiveAppointment = await context.Appointments.AnyAsync(appointment =>
            appointment.DoctorId == slot.DoctorId &&
            appointment.Status != AppointmentStatus.Cancelled &&
            appointment.ScheduledAtUtc < slot.EndAtUtc &&
            slot.StartAtUtc < appointment.ScheduledEndAtUtc);
        if (hasActiveAppointment)
        {
            return false;
        }

        slot.IsAvailable = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> OverlapsExistingAsync(int doctorId, DateTime start, DateTime end)
    {
        ValidateRange(start, end);
        return await _mainRepo.Context.AvailabilitySlots.AnyAsync(slot =>
            slot.DoctorId == doctorId &&
            slot.StartAtUtc < end &&
            start < slot.EndAtUtc);
    }

    public async Task<IReadOnlyList<AvailabilitySlot>> GenerateSlotsAsync(
        int doctorId,
        SlotGenerationRule rule,
        DateTime fromUtc,
        DateTime toUtc)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ValidateRange(fromUtc, toUtc);
        if (rule.DaysOfWeek.Count == 0)
        {
            throw new ArgumentException("At least one day of the week is required.", nameof(rule));
        }

        if (rule.StartTimeOfDayUtc >= rule.EndTimeOfDayUtc)
        {
            throw new ArgumentException("The rule end time must be later than the start time.", nameof(rule));
        }

        if (rule.SlotDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(rule), "Slot duration must be positive.");
        }

        var context = _mainRepo.Context;
        if (!await context.Doctors.AnyAsync(doctor => doctor.Id == doctorId))
        {
            throw new KeyNotFoundException($"Doctor {doctorId} was not found.");
        }

        var slots = new List<AvailabilitySlot>();
        for (var date = fromUtc.Date; date < toUtc; date = date.AddDays(1))
        {
            if (!rule.DaysOfWeek.Contains(date.DayOfWeek))
            {
                continue;
            }

            var dayStart = DateTime.SpecifyKind(date + rule.StartTimeOfDayUtc.ToTimeSpan(), DateTimeKind.Utc);
            var dayEnd = DateTime.SpecifyKind(date + rule.EndTimeOfDayUtc.ToTimeSpan(), DateTimeKind.Utc);

            for (var start = dayStart; start + rule.SlotDuration <= dayEnd; start += rule.SlotDuration)
            {
                var end = start + rule.SlotDuration;
                if (start < fromUtc || end > toUtc)
                {
                    continue;
                }

                var overlaps = await context.AvailabilitySlots.AnyAsync(slot =>
                    slot.DoctorId == doctorId &&
                    slot.StartAtUtc < end &&
                    start < slot.EndAtUtc);
                if (!overlaps)
                {
                    slots.Add(new AvailabilitySlot
                    {
                        DoctorId = doctorId,
                        StartAtUtc = start,
                        EndAtUtc = end,
                        IsAvailable = true
                    });
                }
            }
        }

        if (slots.Count > 0)
        {
            await _mainRepo.AddRangeAsync(slots);
        }

        return slots;
    }

    public async Task<int> RemoveFutureUnbookedAsync(int doctorId)
    {
        var context = _mainRepo.Context;
        var now = DateTime.UtcNow;
        var slots = await context.AvailabilitySlots
            .Where(slot => slot.DoctorId == doctorId && slot.IsAvailable && slot.StartAtUtc >= now)
            .ToListAsync();
        if (slots.Count == 0)
        {
            return 0;
        }

        await _mainRepo.DeleteRangeAsync(slots);
        return slots.Count;
    }

    public async Task<IReadOnlyList<AvailabilitySlot>> GetBookedSlotsInRangeAsync(
        int doctorId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        ValidateRange(fromUtc, toUtc);
        return await _mainRepo.Context.AvailabilitySlots
            .AsNoTracking()
            .Where(slot =>
                slot.DoctorId == doctorId &&
                slot.StartAtUtc < toUtc &&
                fromUtc < slot.EndAtUtc &&
                _mainRepo.Context.Appointments.Any(appointment =>
                    appointment.DoctorId == doctorId &&
                    appointment.Status != AppointmentStatus.Cancelled &&
                    appointment.ScheduledAtUtc < slot.EndAtUtc &&
                    slot.StartAtUtc < appointment.ScheduledEndAtUtc))
            .OrderBy(slot => slot.StartAtUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AvailabilitySlot>> GetAvailableAsync(int doctorId, DateTime dateUtc)
    {
        var fromUtc = DateTime.SpecifyKind(dateUtc.Date, DateTimeKind.Utc);
        var toUtc = fromUtc.AddDays(1);
        return await _mainRepo.Context.AvailabilitySlots
            .AsNoTracking()
            .Where(slot =>
                slot.DoctorId == doctorId &&
                slot.IsAvailable &&
                slot.StartAtUtc < toUtc &&
                fromUtc < slot.EndAtUtc)
            .OrderBy(slot => slot.StartAtUtc)
            .ToListAsync();
    }

    private static void ValidateRange(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            throw new ArgumentException("The end time must be later than the start time.");
        }
    }
}
