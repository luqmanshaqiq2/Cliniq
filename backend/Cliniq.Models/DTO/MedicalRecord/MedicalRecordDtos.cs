using Backend.Cliniq.Models.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Backend.Cliniq.Models.DTO.MedicalRecord;

public sealed class MedicalRecordCreateDto
{
    [Required, StringLength(4000, MinimumLength = 1)]
    public string Diagnosis { get; set; } = string.Empty;
    [StringLength(4000)]
    public string? Prescription { get; set; }
    [EnumDataType(typeof(TreatmentType))]
    public TreatmentType TreatmentType { get; set; }
    [StringLength(4000)]
    public string? SurgeryDetails { get; set; }
    [StringLength(4000)]
    public string? Notes { get; set; }
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }
    [Range(1, int.MaxValue)]
    public int AppointmentId { get; set; }
}

public sealed class MedicalRecordUpdateDto
{
    [Required, StringLength(4000, MinimumLength = 1)]
    public string Diagnosis { get; set; } = string.Empty;
    [StringLength(4000)]
    public string? Prescription { get; set; }
    [EnumDataType(typeof(TreatmentType))]
    public TreatmentType TreatmentType { get; set; }
    [StringLength(4000)]
    public string? SurgeryDetails { get; set; }
    [StringLength(4000)]
    public string? Notes { get; set; }
}

public sealed  class MedicalRecordResponseDto
{
    public int Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Prescription { get; set; }
    public TreatmentType TreatmentType { get; set; }
    public string? SurgeryDetails { get; set; }
    public string? Notes { get; set; }
    public int PatientId { get; set; }
    public int AppointmentId { get; set; }
    public int? ApprovedByDoctorId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
}
