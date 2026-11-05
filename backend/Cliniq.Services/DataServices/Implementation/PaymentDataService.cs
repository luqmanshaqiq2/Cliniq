using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Interfaces;
using Cliniq.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.Services.DataServices.Implementation;

public sealed class PaymentDataService
    : BaseDataService.BaseDalDataService<Payment, PaymentDataService>,
      IPaymentDataService
{
    public PaymentDataService(IPaymentRepo repository, IAppLogging<PaymentDataService> logger)
        : base(repository, logger)
    {
    }

    public Task<Payment> RecordPaymentAsync(
        int invoiceId,
        decimal amount,
        PaymentMethod method,
        string idempotencyKey,
        string? gatewayReference = null)
    {
        return RecordAsync(invoiceId, amount, method, idempotencyKey, gatewayReference, partial: false);
    }

    public async Task<decimal> GetTotalPaidAsync(int invoiceId)
    {
        var amount = await _mainRepo.Context.Payments
            .Where(payment => payment.InvoiceId == invoiceId && payment.Status == PaymentStatus.Completed)
            .SumAsync(payment => (decimal?)payment.Amount);
        return amount ?? 0m;
    }

    public Task<Payment> RecordPartialAsync(
        int invoiceId,
        decimal amount,
        PaymentMethod method,
        string idempotencyKey,
        string? gatewayReference = null)
    {
        return RecordAsync(invoiceId, amount, method, idempotencyKey, gatewayReference, partial: true);
    }

    public async Task<Payment?> RefundAsync(int paymentId, decimal amount)
    {
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        var payment = await _mainRepo.Context.Payments
            .SingleOrDefaultAsync(item => item.Id == paymentId);
        if (payment is null)
        {
            return null;
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            throw new InvalidOperationException("Only completed payments can be refunded.");
        }

        if (amount != payment.Amount)
        {
            throw new NotSupportedException(
                "Partial refunds cannot be represented because the current payment model has no refund record or parent-payment link.");
        }

        payment.Status = PaymentStatus.Refunded;
        await _mainRepo.Context.SaveChangesAsync();
        return payment;
    }

    public Task<Payment?> GetByGatewayReferenceAsync(string gatewayReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gatewayReference);
        return _mainRepo.Context.Payments
            .SingleOrDefaultAsync(payment => payment.TransactionReference == gatewayReference);
    }

    public async Task<bool> MarkFailedAsync(int paymentId)
    {
        var payment = await _mainRepo.Context.Payments
            .SingleOrDefaultAsync(item => item.Id == paymentId);
        if (payment is null)
        {
            return false;
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return false;
        }

        payment.Status = PaymentStatus.Failed;
        await _mainRepo.Context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<Payment>> ReconcileAsync(DateTime fromUtc, DateTime toUtc)
    {
        ValidateRange(fromUtc, toUtc);
        return await _mainRepo.Context.Payments
            .AsNoTracking()
            .Where(payment =>
                payment.Status == PaymentStatus.Pending &&
                payment.PaidAtUtc >= fromUtc &&
                payment.PaidAtUtc < toUtc)
            .OrderBy(payment => payment.PaidAtUtc)
            .ToListAsync();
    }

    private async Task<Payment> RecordAsync(
        int invoiceId,
        decimal amount,
        PaymentMethod method,
        string idempotencyKey,
        string? gatewayReference,
        bool partial)
    {
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (!string.IsNullOrWhiteSpace(gatewayReference))
        {
            throw new NotSupportedException(
                "The current payment model cannot persist both a gateway reference and a separate idempotency key.");
        }

        var reference = idempotencyKey.Trim();
        var context = _mainRepo.Context;
        var existing = await context.Payments
            .SingleOrDefaultAsync(payment => payment.TransactionReference == reference);
        if (existing is not null)
        {
            if (existing.InvoiceId == invoiceId &&
                existing.Amount == amount &&
                existing.Method == method)
            {
                return existing;
            }

            throw new InvalidOperationException(
                "The transaction reference is already associated with a different payment.");
        }

        var invoice = await context.Invoices
            .Include(item => item.Payments)
            .SingleOrDefaultAsync(item => item.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");
        if (invoice.Status is InvoiceStatus.Void or InvoiceStatus.Draft)
        {
            throw new InvalidOperationException("Payments cannot be recorded for draft or void invoices.");
        }

        var paid = invoice.Payments
            .Where(payment => payment.Status == PaymentStatus.Completed)
            .Sum(payment => payment.Amount);
        var outstanding = invoice.Amount - paid;
        if (outstanding <= 0m)
        {
            throw new InvalidOperationException("The invoice has no outstanding balance.");
        }

        if (partial ? amount >= outstanding : amount != outstanding)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                partial
                    ? "A partial payment must be less than the outstanding balance."
                    : "A full payment must equal the outstanding balance.");
        }

        var payment = new Payment
        {
            InvoiceId = invoiceId,
            Amount = amount,
            Method = method,
            TransactionReference = reference,
            PaidAtUtc = DateTime.UtcNow,
            Status = PaymentStatus.Completed
        };
        await context.Payments.AddAsync(payment);

        if (amount == outstanding)
        {
            invoice.Status = InvoiceStatus.Paid;
        }

        await context.SaveChangesAsync();
        return payment;
    }

    private static void ValidateRange(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            throw new ArgumentException("The end time must be later than the start time.");
        }
    }
}
