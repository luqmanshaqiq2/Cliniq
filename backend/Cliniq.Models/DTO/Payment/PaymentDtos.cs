using Backend.Cliniq.Models.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Backend.Cliniq.Models.DTO.Payment;

public sealed class PaymentCreateDto
{
    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }
    [EnumDataType(typeof(PaymentMethod))]
    public PaymentMethod Method { get; set; }
    [StringLength(200)]
    public string? TransactionReference { get; set; }
    [Range(1, int.MaxValue)]
    public int InvoiceId { get; set; }
}

public sealed class PaymentResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAtUtc { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public int InvoiceId { get; set; }
}
