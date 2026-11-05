using Cliniq.API.Contracts;
namespace Cliniq.API.Controllers;

public sealed class InvoicesController
    : BaseCrudController<Invoice, InvoiceCreateDto, InvoiceUpdateDto, InvoiceResponseDto>
{
    private readonly IInvoiceDataService _invoices;
    private readonly IPaymentDataService _payments;
    private readonly IPatientDataService _patients;
    private readonly IAppointmentDataService _appointments;

    public InvoicesController(
        IInvoiceDataService invoices,
        IPaymentDataService payments,
        IPatientDataService patients,
        IAppointmentDataService appointments)
        : base(invoices)
    {
        _invoices = invoices;
        _payments = payments;
        _patients = patients;
        _appointments = appointments;
    }

    [HttpGet("next-number")]
    public async Task<ActionResult<string>> NextInvoiceNumber()
    {
        return Ok(await _invoices.NextInvoiceNumberAsync());
    }

    [HttpGet("outstanding/{patientId:int}")]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponseDto>>> GetOutstanding(int patientId)
    {
        var invoices = await _invoices.GetOutstandingAsync(patientId);
        return Ok(invoices.Select(ToResponse).ToArray());
    }

    [HttpPost("overdue/refresh")]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponseDto>>> RefreshOverdue(
        [FromQuery] DateTime? asOfUtc)
    {
        var invoices = await _invoices.GetOverdueAsync(asOfUtc ?? DateTime.UtcNow);
        return Ok(invoices.Select(ToResponse).ToArray());
    }

    [HttpPost("{id:int}/void")]
    public async Task<ActionResult<InvoiceResponseDto>> Void(int id, InvoiceVoidRequest request)
    {
        var invoice = await _invoices.VoidAsync(id, request.Reason);
        return invoice is null ? NotFound() : Ok(ToResponse(invoice));
    }

    public override Task<IActionResult> Delete(int id)
    {
        throw MethodNotAllowed("Invoice deletion");
    }

    protected override async Task<Invoice> CreateEntityAsync(InvoiceCreateDto input)
    {
        if (input.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(input.Amount));
        }

        if (input.Status is not (InvoiceStatus.Draft or InvoiceStatus.Issued))
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.Status),
                "New invoices must be Draft or Issued; payment and void states use dedicated operations.");
        }

        if (await _patients.FindAsync(input.PatientId) is null)
        {
            throw new KeyNotFoundException($"Patient {input.PatientId} was not found.");
        }

        var invoice = MapCreate(input);
        if (invoice.AppointmentId.HasValue)
        {
            var appointment = await _appointments.FindAsync(invoice.AppointmentId.Value);
            if (appointment is null)
            {
                throw new KeyNotFoundException($"Appointment {invoice.AppointmentId.Value} was not found.");
            }

            if (appointment.PatientId != invoice.PatientId)
            {
                throw new BaseWebException(
                    "The invoice patient must match the appointment patient.",
                    StatusCodes.Status400BadRequest,
                    "appointment_patient_mismatch");
            }

            if (await _invoices.ExistsForAppointmentAsync(invoice.AppointmentId.Value))
            {
                throw new BaseWebException(
                    "An invoice already exists for this appointment.",
                    StatusCodes.Status409Conflict,
                    "invoice_already_exists");
            }
        }

        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            invoice.InvoiceNumber = await _invoices.NextInvoiceNumberAsync();
        }

        return await _invoices.AddAsync(invoice);
    }

    protected override Invoice MapCreate(InvoiceCreateDto input)
    {
        return new Invoice
        {
            InvoiceNumber = input.InvoiceNumber.Trim(),
            Amount = input.Amount,
            IssuedAtUtc = DateTime.UtcNow,
            DueAtUtc = input.DueAtUtc,
            Status = input.Status,
            PatientId = input.PatientId,
            AppointmentId = input.AppointmentId
        };
    }

    protected override void ApplyUpdate(Invoice entity, InvoiceUpdateDto input)
    {
        if (input.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(input.Amount));
        }

        if (input.Status is not (InvoiceStatus.Draft or InvoiceStatus.Issued or InvoiceStatus.Overdue) ||
            entity.Status is InvoiceStatus.Paid or InvoiceStatus.Void)
        {
            throw new BaseWebException(
                "Invoice payment and void states cannot be changed through the update endpoint.",
                StatusCodes.Status409Conflict,
                "invalid_invoice_transition");
        }

        entity.Amount = input.Amount;
        entity.DueAtUtc = input.DueAtUtc;
        entity.Status = input.Status;
    }

    protected override InvoiceResponseDto ToResponse(Invoice entity)
    {
        return new InvoiceResponseDto
        {
            Id = entity.Id,
            InvoiceNumber = entity.InvoiceNumber,
            Amount = entity.Amount,
            IssuedAtUtc = entity.IssuedAtUtc,
            DueAtUtc = entity.DueAtUtc,
            Status = entity.Status,
            PatientId = entity.PatientId,
            AppointmentId = entity.AppointmentId
        };
    }

    public override async Task<ActionResult<InvoiceResponseDto>> Update(int id, InvoiceUpdateDto input)
    {
        var existing = await _invoices.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (input.Amount < await _payments.GetTotalPaidAsync(id))
        {
            throw new BaseWebException(
                "The invoice amount cannot be lower than the amount already paid.",
                StatusCodes.Status409Conflict,
                "invoice_amount_below_paid_total");
        }

        ApplyUpdate(existing, input);
        await _invoices.UpdateAsync(existing);
        return Ok(ToResponse(existing));
    }
}
