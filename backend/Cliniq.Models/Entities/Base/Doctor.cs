using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities.Base
{
    [EntityTypeConfiguration(typeof(DoctorConfiguration))]
    public class Doctor : BaseEntity
    {
    public string FullName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
    }
}