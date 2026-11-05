using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.Base;

namespace Cliniq.DataAccess.Repos.interfaces
{
    public interface IInvoiceRepo : IBaseRepo<Invoice>
    {
        Task<decimal?> GetOutstandingBalanceAsync(int invoiceId);

        Task<IReadOnlyList<Invoice>> GetForPatientAsync(int patientId);
    }
}
