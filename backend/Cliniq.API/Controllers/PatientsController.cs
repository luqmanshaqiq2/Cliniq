namespace Cliniq.API.Controllers;

public sealed class PatientsController
    : BaseCrudController<Patient, PatientCreateDto, PatientUpdateDto, PatientResponseDto>
{
    private readonly IPatientDataService _patients;

    public PatientsController(IPatientDataService patients) : base(patients)
    {
        _patients = patients;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<PatientResponseDto>>> Search(
        [FromQuery] string term,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1)
    {
        ValidatePage(pageSize, pageNumber);
        var patients = await _patients.SearchAsync(term, pageSize, pageNumber);
        return Ok(patients.Select(ToResponse).ToArray());
    }

    [HttpGet("possible-duplicates")]
    public async Task<ActionResult<IReadOnlyList<PatientResponseDto>>> FindPossibleDuplicates(
        [FromQuery] string name,
        [FromQuery] DateTime dateOfBirth,
        [FromQuery] string phone)
    {
        var patients = await _patients.FindPossibleDuplicatesAsync(name, dateOfBirth, phone);
        return Ok(patients.Select(ToResponse).ToArray());
    }

    [HttpPost("{primaryId:int}/merge/{duplicateId:int}")]
    public async Task<ActionResult<PatientResponseDto>> Merge(int primaryId, int duplicateId)
    {
        var patient = await _patients.MergeAsync(primaryId, duplicateId);
        return patient is null ? NotFound() : Ok(ToResponse(patient));
    }

    [HttpPost("{id:int}/anonymize")]
    public async Task<IActionResult> Anonymize(int id)
    {
        return await _patients.AnonymizeAsync(id) ? NoContent() : NotFound();
    }

    public override async Task<IActionResult> Delete(int id)
    {
        return await Anonymize(id);
    }

    protected override Patient MapCreate(PatientCreateDto input)
    {
        return new Patient
        {
            FullName = input.FullName.Trim(),
            DateOfBirth = input.DateOfBirth,
            ContactNumber = input.ContactNumber.Trim(),
            Email = input.Email?.Trim(),
            Address = input.Address?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    protected override void ApplyUpdate(Patient entity, PatientUpdateDto input)
    {
        entity.FullName = input.FullName.Trim();
        entity.DateOfBirth = input.DateOfBirth;
        entity.ContactNumber = input.ContactNumber.Trim();
        entity.Email = input.Email?.Trim();
        entity.Address = input.Address?.Trim();
    }

    protected override PatientResponseDto ToResponse(Patient entity)
    {
        return new PatientResponseDto
        {
            Id = entity.Id,
            FullName = entity.FullName,
            DateOfBirth = entity.DateOfBirth,
            ContactNumber = entity.ContactNumber,
            Email = entity.Email,
            Address = entity.Address,
            CreatedAt = entity.CreatedAt
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
