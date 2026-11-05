using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities.Base;

[EntityTypeConfiguration(typeof(AvailabilitySlotConfiguration))]
public class AvailabilitySlot : BaseEntity
{
    public int DoctorId { get; set; }
    public virtual Doctor Doctor { get; set; } = null!;

    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public bool IsAvailable { get; set; } = true;
}
