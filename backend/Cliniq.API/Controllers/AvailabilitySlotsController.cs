using Cliniq.API.Contracts;
namespace Cliniq.API.Controllers;

public sealed class AvailabilitySlotsController
    : BaseCrudController<
        AvailabilitySlot,
        AvailabilitySlotCreateDto,
        AvailabilitySlotUpdateDto,
        AvailabilitySlotResponseDto>
{
    private readonly IAvailabilitySlotDataService _slots;
    private readonly IDoctorDataService _doctors;

    public AvailabilitySlotsController(
        IAvailabilitySlotDataService slots,
        IDoctorDataService doctors)
        : base(slots)
    {
        _slots = slots;
        _doctors = doctors;
    }

    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<AvailabilitySlotResponseDto>>> GetAvailable(
        [FromQuery] int doctorId,
        [FromQuery] DateTime dateUtc)
    {
        var slots = await _slots.GetAvailableAsync(doctorId, dateUtc);
        return Ok(slots.Select(ToResponse).ToArray());
    }

    [HttpGet("booked")]
    public async Task<ActionResult<IReadOnlyList<AvailabilitySlotResponseDto>>> GetBooked(
        [FromQuery] int doctorId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var slots = await _slots.GetBookedSlotsInRangeAsync(doctorId, fromUtc, toUtc);
        return Ok(slots.Select(ToResponse).ToArray());
    }

    [HttpPost("generate")]
    public async Task<ActionResult<IReadOnlyList<AvailabilitySlotResponseDto>>> Generate(
        [FromQuery] int doctorId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        SlotGenerationRequest request)
    {
        if (toUtc <= fromUtc || toUtc - fromUtc > TimeSpan.FromDays(366))
        {
            throw new ArgumentOutOfRangeException(
                nameof(toUtc),
                "The generation range must be positive and no longer than 366 days.");
        }

        if (request.DaysOfWeek.Any(day => !Enum.IsDefined(day)))
        {
            throw new ArgumentOutOfRangeException(nameof(request.DaysOfWeek));
        }

        var rule = new SlotGenerationRule(
            request.DaysOfWeek,
            request.StartTimeOfDayUtc,
            request.EndTimeOfDayUtc,
            request.SlotDuration);
        var slots = await _slots.GenerateSlotsAsync(doctorId, rule, fromUtc, toUtc);
        return Ok(slots.Select(ToResponse).ToArray());
    }

    [HttpPost("{id:int}/claim")]
    public async Task<IActionResult> Claim(int id)
    {
        return await _slots.ClaimSlotAsync(id) ? NoContent() : Conflict();
    }

    [HttpPost("{id:int}/release")]
    public async Task<IActionResult> Release(int id)
    {
        if (await _slots.ReleaseSlotAsync(id))
        {
            return NoContent();
        }

        return await _slots.FindAsync(id) is null ? NotFound() : Conflict();
    }

    [HttpDelete("future-unbooked")]
    public async Task<ActionResult<int>> RemoveFutureUnbooked([FromQuery] int doctorId)
    {
        return Ok(await _slots.RemoveFutureUnbookedAsync(doctorId));
    }

    public override Task<ActionResult<AvailabilitySlotResponseDto>> Update(
        int id,
        AvailabilitySlotUpdateDto input)
    {
        throw MethodNotAllowed("Direct availability-slot updates");
    }

    public override Task<IActionResult> Delete(int id)
    {
        throw MethodNotAllowed("Availability-slot deletion");
    }

    protected override async Task<AvailabilitySlot> CreateEntityAsync(AvailabilitySlotCreateDto input)
    {
        if (await _doctors.FindAsync(input.DoctorId) is null)
        {
            throw new KeyNotFoundException($"Doctor {input.DoctorId} was not found.");
        }

        if (await _slots.OverlapsExistingAsync(input.DoctorId, input.StartAtUtc, input.EndAtUtc))
        {
            throw new BaseWebException(
                "The availability slot overlaps an existing slot.",
                StatusCodes.Status409Conflict,
                "slot_overlap");
        }

        return await _slots.AddAsync(new AvailabilitySlot
        {
            DoctorId = input.DoctorId,
            StartAtUtc = input.StartAtUtc,
            EndAtUtc = input.EndAtUtc,
            IsAvailable = true
        });
    }

    protected override void ApplyUpdate(AvailabilitySlot entity, AvailabilitySlotUpdateDto input)
    {
        entity.StartAtUtc = input.StartAtUtc;
        entity.EndAtUtc = input.EndAtUtc;
        entity.IsAvailable = input.IsAvailable;
    }

    protected override AvailabilitySlotResponseDto ToResponse(AvailabilitySlot entity)
    {
        return new AvailabilitySlotResponseDto
        {
            Id = entity.Id,
            DoctorId = entity.DoctorId,
            StartAtUtc = entity.StartAtUtc,
            EndAtUtc = entity.EndAtUtc,
            IsAvailable = entity.IsAvailable
        };
    }
}
