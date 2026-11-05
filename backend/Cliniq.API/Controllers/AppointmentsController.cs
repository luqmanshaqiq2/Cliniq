using System.Security.Claims;
namespace Cliniq.API.Controllers;

public sealed class AppointmentsController
    : BaseCrudController<Appointment, AppointmentCreateDto, AppointmentUpdateDto, AppointmentResponseDto>
{
    private readonly IAppointmentDataService _appointments;
    private readonly IAvailabilitySlotDataService _slots;

    public AppointmentsController(
        IAppointmentDataService appointments,
        IAvailabilitySlotDataService slots)
        : base(appointments)
    {
        _appointments = appointments;
        _slots = slots;
    }

    [HttpGet("by-status-range")]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponseDto>>> GetByStatusAndRange(
        [FromQuery] AppointmentStatus status,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1)
    {
        ValidatePage(pageSize, pageNumber);
        var appointments = await _appointments.GetByStatusAndRangeAsync(
            status, fromUtc, toUtc, pageSize, pageNumber);
        return Ok(appointments.Select(ToResponse).ToArray());
    }

    [HttpPost("{id:int}/reschedule")]
    public async Task<ActionResult<AppointmentResponseDto>> Reschedule(
        int id,
        [FromQuery] int newSlotId)
    {
        var result = await _appointments.RescheduleAsync(id, newSlotId);
        return MapBookingResult(result);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, AppointmentCancellationRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var result = await _appointments.CancelAsync(id, request.Reason, userId);
        return result switch
        {
            AppointmentTransitionStatus.Updated => NoContent(),
            AppointmentTransitionStatus.AppointmentNotFound => NotFound(),
            _ => Conflict()
        };
    }

    [HttpPost("{id:int}/no-show")]
    public async Task<IActionResult> MarkNoShow(int id)
    {
        var result = await _appointments.MarkNoShowAsync(id);
        return result switch
        {
            AppointmentTransitionStatus.Updated => NoContent(),
            AppointmentTransitionStatus.AppointmentNotFound => NotFound(),
            _ => Conflict()
        };
    }

    [HttpGet("upcoming-reminders")]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponseDto>>> GetUpcomingForReminder(
        [FromQuery] int windowMinutes = 60)
    {
        if (windowMinutes < 1 || windowMinutes > 10080)
        {
            throw new ArgumentOutOfRangeException(nameof(windowMinutes));
        }

        var appointments = await _appointments.GetUpcomingForReminderAsync(
            TimeSpan.FromMinutes(windowMinutes));
        return Ok(appointments.Select(ToResponse).ToArray());
    }

    public override Task<ActionResult<AppointmentResponseDto>> Update(
        int id,
        AppointmentUpdateDto input)
    {
        throw MethodNotAllowed("Direct appointment updates");
    }

    public override Task<IActionResult> Delete(int id)
    {
        throw MethodNotAllowed("Appointment deletion");
    }

    protected override async Task<Appointment> CreateEntityAsync(AppointmentCreateDto input)
    {
        var availableSlots = await _slots.GetAvailableAsync(input.DoctorId, input.ScheduledAtUtc);
        var slot = availableSlots.SingleOrDefault(candidate =>
            candidate.StartAtUtc == input.ScheduledAtUtc &&
            candidate.EndAtUtc == input.ScheduledEndAtUtc);
        if (slot is null)
        {
            throw new BaseWebException(
                "The requested time does not match an available doctor slot.",
                StatusCodes.Status409Conflict,
                "slot_unavailable");
        }

        var result = await _appointments.TryBookAsync(input.PatientId, slot.Id, input.Notes);
        if (result.Status == AppointmentBookingStatus.Booked && result.Appointment is not null)
        {
            return result.Appointment;
        }

        throw BookingConflict(result.Status);
    }

    protected override void ApplyUpdate(Appointment entity, AppointmentUpdateDto input)
    {
        throw MethodNotAllowed("Direct appointment updates");
    }

    protected override AppointmentResponseDto ToResponse(Appointment entity)
    {
        return new AppointmentResponseDto
        {
            Id = entity.Id,
            ScheduledAtUtc = entity.ScheduledAtUtc,
            ScheduledEndAtUtc = entity.ScheduledEndAtUtc,
            Notes = entity.Notes,
            Status = entity.Status,
            PatientId = entity.PatientId,
            DoctorId = entity.DoctorId,
            CreatedAt = entity.CreatedAt
        };
    }

    private ActionResult<AppointmentResponseDto> MapBookingResult(AppointmentBookingResult result)
    {
        if (result.Status == AppointmentBookingStatus.Booked && result.Appointment is not null)
        {
            return Ok(ToResponse(result.Appointment));
        }

        throw BookingConflict(result.Status);
    }

    private static BaseWebException BookingConflict(AppointmentBookingStatus status)
    {
        return status switch
        {
            AppointmentBookingStatus.AppointmentNotFound => new BaseWebException(
                "The appointment or slot was not found.",
                StatusCodes.Status404NotFound,
                "appointment_or_slot_not_found"),
            AppointmentBookingStatus.SlotTaken => new BaseWebException(
                "The availability slot is no longer available.",
                StatusCodes.Status409Conflict,
                "slot_unavailable"),
            AppointmentBookingStatus.PatientOverlap => new BaseWebException(
                "The patient has an overlapping appointment.",
                StatusCodes.Status409Conflict,
                "patient_appointment_overlap"),
            _ => new BaseWebException(
                "The appointment cannot be booked or rescheduled in its current state.",
                StatusCodes.Status409Conflict,
                "invalid_appointment_state")
        };
    }

    private static void ValidatePage(int pageSize, int pageNumber)
    {
        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }
    }
}
