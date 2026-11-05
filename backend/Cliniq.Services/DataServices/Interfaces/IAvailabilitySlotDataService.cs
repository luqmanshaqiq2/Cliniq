using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface IAvailabilitySlotDataService : IBaseDataService<AvailabilitySlot>
    {
        Task<bool> ClaimSlotAsync(int slotId);

        Task<bool> ReleaseSlotAsync(int slotId);

        Task<bool> OverlapsExistingAsync(
            int doctorId,
            DateTime start,
            DateTime end);

        Task<IReadOnlyList<AvailabilitySlot>> GenerateSlotsAsync(
            int doctorId,
            SlotGenerationRule rule,
            DateTime fromUtc,
            DateTime toUtc);

        Task<int> RemoveFutureUnbookedAsync(int doctorId);

        Task<IReadOnlyList<AvailabilitySlot>> GetBookedSlotsInRangeAsync(
            int doctorId,
            DateTime fromUtc,
            DateTime toUtc);

        Task<IReadOnlyList<AvailabilitySlot>> GetAvailableAsync(
            int doctorId,
            DateTime dateUtc);
    }
}
