using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Services.DataServices.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface IInvoiceDataService : IBaseDataService<Invoice>
    {
        Task<bool> ExistsForAppointmentAsync(int appointmentId);

        Task<IReadOnlyList<Invoice>> GetOutstandingAsync(int patientId);

        Task<IReadOnlyList<Invoice>> GetOverdueAsync(DateTime asOf);

        Task<Invoice?> VoidAsync(int invoiceId, string reason);

        Task<decimal> ApplyCreditNoteAsync(int invoiceId, decimal amount);

        Task<decimal> RecalculateTotalAsync(int invoiceId);

        Task<string> NextInvoiceNumberAsync();
    }
}
