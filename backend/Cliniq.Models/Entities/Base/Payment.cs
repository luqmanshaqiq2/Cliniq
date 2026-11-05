using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities.Base
{
    [EntityTypeConfiguration(typeof(PaymentConfiguration))]
    public class Payment : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime PaidAtUtc { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
        public string? TransactionReference { get; set; }

        public int InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;
    }

    public enum PaymentMethod
    {
        Cash = 1,
        Card = 2,
        BankTransfer = 3,
        Insurance = 4,
        Other = 5
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }
}