using Cliniq.API.Contracts;
namespace Cliniq.API.Controllers;

public sealed class PaymentsController
    : BaseCrudController<Payment, PaymentCreateDto, EmptyRequest, PaymentResponseDto>
{
    private readonly IPaymentDataService _payments;

    public PaymentsController(IPaymentDataService payments) : base(payments)
    {
        _payments = payments;
    }

    [HttpGet("total-paid/{invoiceId:int}")]
    public async Task<ActionResult<decimal>> GetTotalPaid(int invoiceId)
    {
        return Ok(await _payments.GetTotalPaidAsync(invoiceId));
    }

    [HttpGet("by-reference")]
    public async Task<ActionResult<PaymentResponseDto>> GetByReference([FromQuery] string reference)
    {
        var payment = await _payments.GetByGatewayReferenceAsync(reference);
        return payment is null ? NotFound() : Ok(ToResponse(payment));
    }

    [HttpPost("{id:int}/refund")]
    public async Task<ActionResult<PaymentResponseDto>> Refund(int id, RefundRequest request)
    {
        var payment = await _payments.RefundAsync(id, request.Amount);
        return payment is null ? NotFound() : Ok(ToResponse(payment));
    }

    [HttpPost("{id:int}/failed")]
    public async Task<IActionResult> MarkFailed(int id)
    {
        if (await _payments.MarkFailedAsync(id))
        {
            return NoContent();
        }

        return await _payments.FindAsync(id) is null ? NotFound() : Conflict();
    }

    [HttpGet("reconcile")]
    public async Task<ActionResult<IReadOnlyList<PaymentResponseDto>>> Reconcile(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var payments = await _payments.ReconcileAsync(fromUtc, toUtc);
        return Ok(payments.Select(ToResponse).ToArray());
    }

    [HttpPost("partial")]
    public async Task<ActionResult<PaymentResponseDto>> RecordPartial(PaymentCreateDto input)
    {
        var idempotencyKey = GetIdempotencyKey(input);
        if (!Enum.IsDefined(input.Method))
        {
            throw new ArgumentOutOfRangeException(nameof(input.Method));
        }

        var payment = await _payments.RecordPartialAsync(
            input.InvoiceId,
            input.Amount,
            input.Method,
            idempotencyKey);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, ToResponse(payment));
    }

    public override Task<ActionResult<PaymentResponseDto>> Update(int id, EmptyRequest input)
    {
        throw MethodNotAllowed("Payment updates");
    }

    public override Task<IActionResult> Delete(int id)
    {
        throw MethodNotAllowed("Payment deletion");
    }

    protected override async Task<Payment> CreateEntityAsync(PaymentCreateDto input)
    {
        if (!Enum.IsDefined(input.Method))
        {
            throw new ArgumentOutOfRangeException(nameof(input.Method));
        }

        var payment = await _payments.RecordPaymentAsync(
            input.InvoiceId,
            input.Amount,
            input.Method,
            GetIdempotencyKey(input));
        return payment;
    }

    protected override Payment MapCreate(PaymentCreateDto input)
    {
        throw MethodNotAllowed("Direct payment creation");
    }

    protected override void ApplyUpdate(Payment entity, EmptyRequest input)
    {
        throw MethodNotAllowed("Payment updates");
    }

    protected override PaymentResponseDto ToResponse(Payment entity)
    {
        return new PaymentResponseDto
        {
            Id = entity.Id,
            Amount = entity.Amount,
            PaidAtUtc = entity.PaidAtUtc,
            Method = entity.Method,
            Status = entity.Status,
            TransactionReference = entity.TransactionReference,
            InvoiceId = entity.InvoiceId
        };
    }

    private string GetIdempotencyKey(PaymentCreateDto input)
    {
        var headerValue = Request.Headers["Idempotency-Key"].FirstOrDefault()?.Trim();
        var dtoValue = input.TransactionReference?.Trim();
        if (!string.IsNullOrWhiteSpace(headerValue) &&
            !string.IsNullOrWhiteSpace(dtoValue) &&
            !string.Equals(headerValue, dtoValue, StringComparison.Ordinal))
        {
            throw new BaseWebException(
                "The Idempotency-Key header and transactionReference must match when both are supplied.",
                StatusCodes.Status400BadRequest,
                "idempotency_key_mismatch");
        }

        var key = string.IsNullOrWhiteSpace(headerValue) ? dtoValue : headerValue;
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BaseWebException(
                "Provide the Idempotency-Key header or transactionReference.",
                StatusCodes.Status400BadRequest,
                "idempotency_key_required");
        }

        return key;
    }
}
