using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities.Base
{
    [EntityTypeConfiguration(typeof(SerilogEntryConfiguration))]
    public class SerilogEntry : BaseEntity
    {
        public DateTime LoggedAtUtc { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? MessageTemplate { get; set; }
        public string? Exception { get; set; }
        public string? Properties { get; set; }
    }
}