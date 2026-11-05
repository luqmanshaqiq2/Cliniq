using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Interfaces;
using Cliniq.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.Services.DataServices.Implementation;

public sealed class InvoiceDataService
    : BaseDataService.BaseDalDataService<Invoice, InvoiceDataService>,
      IInvoiceDataService
{
    public InvoiceDataService(IInvoiceRepo repository, IAppLogging<InvoiceDataService> logger)
        : base(repository, logger)
    {
    }

    public Task<bool> ExistsForAppointmentAsync(int appointmentId)
    {
        return _mainRepo.Context.Invoices
            .AnyAsync(invoice => invoice.AppointmentId == appointmentId);
    }

    public async Task<IReadOnlyList<Invoice>> GetOutstandingAsync(int patientId)
    {
        return await _mainRepo.Context.Invoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.PatientId == patientId &&
                invoice.Status != InvoiceStatus.Void &&
                invoice.Status != InvoiceStatus.Paid &&
                invoice.Amount >
                (invoice.Payments
                    .Where(payment => payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m))
            .OrderBy(invoice => invoice.DueAtUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Invoice>> GetOverdueAsync(DateTime asOf)
    {
        var invoices = await _mainRepo.Context.Invoices
            .Where(invoice =>
                invoice.Status != InvoiceStatus.Void &&
                invoice.Status != InvoiceStatus.Paid &&
                invoice.DueAtUtc.HasValue &&
                invoice.DueAtUtc.Value < asOf &&
                invoice.Amount >
                (invoice.Payments
                    .Where(payment => payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m))
            .OrderBy(invoice => invoice.DueAtUtc)
            .ToListAsync();

        foreach (var invoice in invoices)
        {
            invoice.Status = InvoiceStatus.Overdue;
        }

        if (invoices.Count > 0)
        {
            await _mainRepo.Context.SaveChangesAsync();
        }

        return invoices;
    }

    public async Task<Invoice?> VoidAsync(int invoiceId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        var invoice = await _mainRepo.Context.Invoices
            .SingleOrDefaultAsync(item => item.Id == invoiceId);
        if (invoice is null || invoice.Status == InvoiceStatus.Void)
        {
            return invoice;
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("A paid invoice cannot be voided.");
        }

        invoice.Status = InvoiceStatus.Void;
        _logger.LogInformation("Voided invoice {InvoiceId}. Reason: {Reason}", invoiceId, reason);
        await _mainRepo.Context.SaveChangesAsync();
        return invoice;
    }

    public Task<decimal> ApplyCreditNoteAsync(int invoiceId, decimal amount)
    {
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        throw new NotSupportedException(
            "Credit notes cannot be audited because the current model has no credit-note entity or invoice adjustment history.");
    }

    public Task<decimal> RecalculateTotalAsync(int invoiceId)
    {
        throw new NotSupportedException(
            "Invoice totals cannot be recalculated because the current model has no invoice line items.");
    }

    public async Task<string> NextInvoiceNumberAsync()
    {
        var prefix = $"INV-{DateTime.UtcNow:yyyyMMdd}-";
        var numbers = await _mainRepo.Context.Invoices
            .Where(invoice => invoice.InvoiceNumber.StartsWith(prefix))
            .Select(invoice => invoice.InvoiceNumber)
            .ToListAsync();
        var next = numbers
            .Select(number => int.TryParse(number[prefix.Length..], out var sequence) ? sequence : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{prefix}{next:D4}";
    }
}
