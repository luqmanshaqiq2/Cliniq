using Backend.Cliniq.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.FullName).IsRequired().HasMaxLength(200);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(256);
        builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(30);
        builder.Property(user => user.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(user => user.Email).IsUnique();
    }
}
