using Backend.Cliniq.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Cliniq.Models.DTO.Appointment;

public sealed class AppointmentCreateDto
{
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime ScheduledEndAtUtc { get; set; }
    [StringLength(2000)]
    public string? Notes { get; set; }
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }
}

public sealed class AppointmentUpdateDto
{
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime ScheduledEndAtUtc { get; set; }
    [StringLength(2000)]
    public string? Notes { get; set; }
    [EnumDataType(typeof(AppointmentStatus))]
    public AppointmentStatus Status { get; set; }
}

public sealed class AppointmentResponseDto
{
    public int Id { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime ScheduledEndAtUtc { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime CreatedAt { get; set; }
}
