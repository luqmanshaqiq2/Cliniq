using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Payments_PositiveAmount",
            "CAST(\"Amount\" AS NUMERIC) > 0"));

        builder.Property(payment => payment.Amount).HasPrecision(18, 2);
        builder.Property(payment => payment.PaidAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(payment => payment.Method).HasConversion<string>().HasMaxLength(30);
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(payment => payment.TransactionReference).HasMaxLength(200);

        builder.HasOne(payment => payment.Invoice)
               .WithMany(invoice => invoice.Payments)
               .HasForeignKey(payment => payment.InvoiceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
