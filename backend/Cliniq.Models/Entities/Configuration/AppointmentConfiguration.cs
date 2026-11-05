using Backend.Cliniq.Models.Entities;
using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Appointments_ValidSchedule",
            "\"ScheduledAtUtc\" < \"ScheduledEndAtUtc\""));

        builder.Property(b => b.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.Property(a => a.Status)
               .HasConversion<string>()
               .HasMaxLength(20);

        //appointments has 1 patient - 1 patient has many appointments
        builder.HasOne(a => a.Patient)
               .WithMany(p => p.Appointments)
               .HasForeignKey(a => a.PatientId)
               .OnDelete(DeleteBehavior.Restrict);

        //appointments has 1 doctor - 1 doctor has many appointments
        builder.HasOne(a => a.Doctor)
               .WithMany(d => d.Appointments)
               .HasForeignKey(a => a.DoctorId)
               .OnDelete(DeleteBehavior.Restrict);

        //appointments has 1 medical record
        builder.HasOne(a => a.MedicalRecord)
               .WithOne(m => m.Appointment)
               .HasForeignKey<MedicalRecord>(m => m.AppointmentId);

        // Fast lookups for a doctor's schedule / conflict checks
        builder.HasIndex(a => new { a.DoctorId, a.ScheduledAtUtc });
    }
}
