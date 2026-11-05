using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(m => m.Diagnosis).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Prescription).HasMaxLength(1000);
        builder.Property(m => m.SurgeryDetails).HasMaxLength(2000);
        builder.Property(m => m.Notes).HasMaxLength(1000);

        builder.HasOne(m => m.Patient)
               .WithMany(p => p.MedicalRecords)
               .HasForeignKey(m => m.PatientId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ApprovedByDoctor)
               .WithMany()
               .HasForeignKey(m => m.ApprovedByDoctorId)
               .OnDelete(DeleteBehavior.Restrict);

        // One-to-one with Appointment (FK lives here)
        builder.HasOne(m => m.Appointment)
               .WithOne(a => a.MedicalRecord)
               .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
