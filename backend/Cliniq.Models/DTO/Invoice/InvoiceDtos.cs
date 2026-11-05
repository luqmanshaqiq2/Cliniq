using Backend.Cliniq.Models.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Backend.Cliniq.Models.DTO.Invoice;

public sealed class InvoiceCreateDto
{
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }
    public DateTime? DueAtUtc { get; set; }
    [EnumDataType(typeof(InvoiceStatus))]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }
    [Range(1, int.MaxValue)]
    public int? AppointmentId { get; set; }
}

public sealed class InvoiceUpdateDto
{
    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }
    public DateTime? DueAtUtc { get; set; }
    [EnumDataType(typeof(InvoiceStatus))]
    public InvoiceStatus Status { get; set; }
}

public sealed class InvoiceResponseDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public InvoiceStatus Status { get; set; }
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
}
