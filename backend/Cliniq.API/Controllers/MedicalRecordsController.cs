namespace Cliniq.API.Controllers;

public sealed class MedicalRecordsController
    : BaseCrudController<
        MedicalRecord,
        MedicalRecordCreateDto,
        MedicalRecordUpdateDto,
        MedicalRecordResponseDto>
{
    private readonly IMedicalRecordDataService _records;
    private readonly IAppointmentDataService _appointments;

    public MedicalRecordsController(
        IMedicalRecordDataService records,
        IAppointmentDataService appointments)
        : base(records)
    {
        _records = records;
        _appointments = appointments;
    }

    public override Task<IActionResult> Delete(int id)
    {
        throw MethodNotAllowed("Medical record deletion");
    }

    protected override async Task<MedicalRecord> CreateEntityAsync(MedicalRecordCreateDto input)
    {
        var appointment = await _appointments.FindAsync(input.AppointmentId);
        if (appointment is null)
        {
            throw new KeyNotFoundException($"Appointment {input.AppointmentId} was not found.");
        }

        if (appointment.PatientId != input.PatientId)
        {
            throw new BaseWebException(
                "The medical record patient must match the appointment patient.",
                StatusCodes.Status400BadRequest,
                "appointment_patient_mismatch");
        }

        if (await _records.ExistsForAppointmentAsync(input.AppointmentId))
        {
            throw new BaseWebException(
                "A medical record already exists for this appointment.",
                StatusCodes.Status409Conflict,
                "record_already_exists");
        }

        return await _records.AddAsync(MapCreate(input));
    }

    protected override MedicalRecord MapCreate(MedicalRecordCreateDto input)
    {
        if (!Enum.IsDefined(input.TreatmentType))
        {
            throw new ArgumentOutOfRangeException(nameof(input.TreatmentType));
        }

        return new MedicalRecord
        {
            Diagnosis = input.Diagnosis.Trim(),
            Prescription = input.Prescription?.Trim(),
            TreatmentType = input.TreatmentType,
            SurgeryDetails = input.SurgeryDetails?.Trim(),
            Notes = input.Notes?.Trim(),
            PatientId = input.PatientId,
            AppointmentId = input.AppointmentId,
            CreatedAt = DateTime.UtcNow
        };
    }

    protected override void ApplyUpdate(MedicalRecord entity, MedicalRecordUpdateDto input)
    {
        if (entity.ApprovedAtUtc.HasValue)
        {
            throw new BaseWebException(
                "Approved medical records cannot be changed.",
                StatusCodes.Status409Conflict,
                "record_locked");
        }

        if (!Enum.IsDefined(input.TreatmentType))
        {
            throw new ArgumentOutOfRangeException(nameof(input.TreatmentType));
        }

        entity.Diagnosis = input.Diagnosis.Trim();
        entity.Prescription = input.Prescription?.Trim();
        entity.TreatmentType = input.TreatmentType;
        entity.SurgeryDetails = input.SurgeryDetails?.Trim();
        entity.Notes = input.Notes?.Trim();
    }

    protected override MedicalRecordResponseDto ToResponse(MedicalRecord entity)
    {
        return new MedicalRecordResponseDto
        {
            Id = entity.Id,
            Diagnosis = entity.Diagnosis,
            Prescription = entity.Prescription,
            TreatmentType = entity.TreatmentType,
            SurgeryDetails = entity.SurgeryDetails,
            Notes = entity.Notes,
            PatientId = entity.PatientId,
            AppointmentId = entity.AppointmentId,
            ApprovedByDoctorId = entity.ApprovedByDoctorId,
            ApprovedAtUtc = entity.ApprovedAtUtc,
            CreatedAt = entity.CreatedAt
        };
    }
}
