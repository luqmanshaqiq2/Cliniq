using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(doctor => doctor.FullName).IsRequired().HasMaxLength(200);
        builder.Property(doctor => doctor.Specialization).IsRequired().HasMaxLength(200);
        builder.Property(doctor => doctor.ContactNumber).IsRequired().HasMaxLength(30);
        builder.Property(doctor => doctor.Email).HasMaxLength(256);
        builder.Property(doctor => doctor.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(doctor => doctor.Email)
               .IsUnique()
               .HasFilter("[Email] IS NOT NULL");
    }
}
