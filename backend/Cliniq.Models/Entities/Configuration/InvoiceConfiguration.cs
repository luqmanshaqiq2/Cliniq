using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Invoices_PositiveAmount",
            "CAST(\"Amount\" AS NUMERIC) > 0"));

        builder.Property(invoice => invoice.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(invoice => invoice.Amount).HasPrecision(18, 2);
        builder.Property(invoice => invoice.IssuedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(invoice => invoice.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(invoice => invoice.InvoiceNumber).IsUnique();

        builder.HasOne(invoice => invoice.Patient)
               .WithMany(patient => patient.Invoices)
               .HasForeignKey(invoice => invoice.PatientId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(invoice => invoice.Appointment)
               .WithMany()
               .HasForeignKey(invoice => invoice.AppointmentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
