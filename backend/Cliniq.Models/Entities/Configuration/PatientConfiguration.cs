using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(patient => patient.FullName).IsRequired().HasMaxLength(200);
        builder.Property(patient => patient.ContactNumber).IsRequired().HasMaxLength(30);
        builder.Property(patient => patient.Email).HasMaxLength(256);
        builder.Property(patient => patient.Address).HasMaxLength(500);
        builder.Property(patient => patient.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(patient => patient.Email)
               .IsUnique()
               .HasFilter("[Email] IS NOT NULL");
    }
}
