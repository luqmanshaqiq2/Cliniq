using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_AvailabilitySlots_ValidSchedule",
            "\"StartAtUtc\" < \"EndAtUtc\""));

        builder.HasOne(slot => slot.Doctor)
               .WithMany(doctor => doctor.AvailabilitySlots)
               .HasForeignKey(slot => slot.DoctorId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(slot => new { slot.DoctorId, slot.StartAtUtc, slot.EndAtUtc });
    }
}
