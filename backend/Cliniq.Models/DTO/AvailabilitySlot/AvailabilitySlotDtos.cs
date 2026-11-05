using System.ComponentModel.DataAnnotations;

namespace Backend.Cliniq.Models.DTO.AvailabilitySlot;

public class AvailabilitySlotCreateDto
{
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
}

public class AvailabilitySlotUpdateDto
{
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class AvailabilitySlotResponseDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public bool IsAvailable { get; set; }
}
