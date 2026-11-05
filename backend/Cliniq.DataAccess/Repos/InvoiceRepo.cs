using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Repos.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.DataAccess.Repos
{
    public class InvoiceRepo : BaseRepo<Invoice>, IInvoiceRepo
    {
        public InvoiceRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<decimal?> GetOutstandingBalanceAsync(int invoiceId)
        {
            var invoice = await Table.AsNoTracking()
                .Where(item => item.Id == invoiceId)
                .Select(item => new
                {
                    item.Amount,
                    PaidAmount = item.Payments
                        .Where(payment => payment.Status == PaymentStatus.Completed)
                        .Select(payment => (decimal?)payment.Amount)
                        .Sum() ?? 0m
                })
                .SingleOrDefaultAsync();

            return invoice is null
                ? null
                : Math.Max(0m, invoice.Amount - invoice.PaidAmount);
        }

        public async Task<IReadOnlyList<Invoice>> GetForPatientAsync(int patientId)
        {
            return await Table.AsNoTracking()
                .Where(invoice => invoice.PatientId == patientId)
                .OrderByDescending(invoice => invoice.IssuedAtUtc)
                .ToListAsync();
        }
    }
}
