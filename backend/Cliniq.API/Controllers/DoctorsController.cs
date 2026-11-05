namespace Cliniq.API.Controllers;

public sealed class DoctorsController
    : BaseCrudController<Doctor, DoctorCreateDto, DoctorUpdateDto, DoctorResponseDto>
{
    private readonly IDoctorDataService _doctors;

    public DoctorsController(IDoctorDataService doctors) : base(doctors)
    {
        _doctors = doctors;
    }

    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<DoctorResponseDto>>> GetAvailable(
        [FromQuery] string specialty,
        [FromQuery] DateTime atUtc)
    {
        var doctors = await _doctors.GetAvailableBySpecialtyAsync(specialty, atUtc);
        return Ok(doctors.Select(ToResponse).ToArray());
    }

    [HttpGet("{id:int}/has-active-appointments")]
    public async Task<ActionResult<bool>> HasActiveAppointments(int id)
    {
        if (await _doctors.FindAsync(id) is null)
        {
            return NotFound();
        }

        return Ok(await _doctors.HasActiveAppointmentsAsync(id));
    }

    public override Task<IActionResult> Delete(int id)
    {
        throw MethodNotAllowed("Doctor deletion");
    }

    protected override Doctor MapCreate(DoctorCreateDto input)
    {
        return new Doctor
        {
            FullName = input.FullName.Trim(),
            Specialization = input.Specialization.Trim(),
            ContactNumber = input.ContactNumber.Trim(),
            Email = input.Email?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    protected override void ApplyUpdate(Doctor entity, DoctorUpdateDto input)
    {
        entity.FullName = input.FullName.Trim();
        entity.Specialization = input.Specialization.Trim();
        entity.ContactNumber = input.ContactNumber.Trim();
        entity.Email = input.Email?.Trim();
    }

    protected override DoctorResponseDto ToResponse(Doctor entity)
    {
        return new DoctorResponseDto
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Specialization = entity.Specialization,
            ContactNumber = entity.ContactNumber,
            Email = entity.Email,
            CreatedAt = entity.CreatedAt
        };
    }
}
