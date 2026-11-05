using Backend.Cliniq.Models.Entities;
using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities.Base
{
    [EntityTypeConfiguration(typeof(InvoiceConfiguration))]
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssuedAtUtc { get; set; }
        public DateTime? DueAtUtc { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;

        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public int? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public enum InvoiceStatus
    {
        Draft = 1,
        Issued = 2,
        Paid = 3,
        Overdue = 4,
        Void = 5
    }
}