using Backend.Cliniq.Models.Entities.Base;

namespace Cliniq.Services.DataServices.Contracts
{
    public sealed record MedicalRecordAmendment(
        int AmendedByDoctorId,
        string Reason,
        string? Diagnosis = null,
        string? Prescription = null,
        TreatmentType? TreatmentType = null,
        string? SurgeryDetails = null,
        string? Notes = null);
}
