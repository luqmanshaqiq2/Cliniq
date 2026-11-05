using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Services.DataServices.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface IPaymentDataService : IBaseDataService<Payment>
    {
        Task<Payment> RecordPaymentAsync(
            int invoiceId,
            decimal amount,
            PaymentMethod method,
            string idempotencyKey,
            string? gatewayReference = null);

        Task<decimal> GetTotalPaidAsync(int invoiceId);

        Task<Payment> RecordPartialAsync(
            int invoiceId,
            decimal amount,
            PaymentMethod method,
            string idempotencyKey,
            string? gatewayReference = null);

        Task<Payment?> RefundAsync(int paymentId, decimal amount);

        Task<Payment?> GetByGatewayReferenceAsync(string gatewayReference);

        Task<bool> MarkFailedAsync(int paymentId);

        Task<IReadOnlyList<Payment>> ReconcileAsync(
            DateTime fromUtc,
            DateTime toUtc);
    }
}
