using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.Base;

namespace Cliniq.DataAccess.Repos.interfaces
{
    public interface IPaymentRepo : IBaseRepo<Payment>
    {
        Task<Payment?> FindByTransactionReferenceAsync(string transactionReference);

        Task<IReadOnlyList<Payment>> GetForInvoiceAsync(int invoiceId);
    }
}
