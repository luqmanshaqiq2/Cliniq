using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class SerilogEntryConfiguration : IEntityTypeConfiguration<SerilogEntry>
{
    public void Configure(EntityTypeBuilder<SerilogEntry> builder)
    {
        builder.Property(entry => entry.Level).IsRequired().HasMaxLength(20);
        builder.Property(entry => entry.Message).IsRequired().HasMaxLength(4000);
        builder.Property(entry => entry.MessageTemplate).HasMaxLength(4000);

        builder.HasIndex(entry => entry.LoggedAtUtc);
    }
}
