using Backend.Cliniq.Models.Entities.Base;
using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities
{
    [EntityTypeConfiguration(typeof(AppointmentConfiguration))]
    public class Appointment : BaseEntity
    {
        public DateTime ScheduledAtUtc { get; set; }
        public DateTime ScheduledEndAtUtc { get; set; }
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;

        public virtual MedicalRecord? MedicalRecord { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    // AppointmentStatus.cs
public enum AppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}
}