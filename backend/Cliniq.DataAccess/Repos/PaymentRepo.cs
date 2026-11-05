using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Repos.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.DataAccess.Repos
{
    public class PaymentRepo : BaseRepo<Payment>, IPaymentRepo
    {
        public PaymentRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Payment?> FindByTransactionReferenceAsync(string transactionReference)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(transactionReference);

            return await Table.AsNoTracking()
                .SingleOrDefaultAsync(payment =>
                    payment.TransactionReference == transactionReference);
        }

        public async Task<IReadOnlyList<Payment>> GetForInvoiceAsync(int invoiceId)
        {
            return await Table.AsNoTracking()
                .Where(payment => payment.InvoiceId == invoiceId)
                .OrderBy(payment => payment.PaidAtUtc)
                .ToListAsync();
        }
    }
}
