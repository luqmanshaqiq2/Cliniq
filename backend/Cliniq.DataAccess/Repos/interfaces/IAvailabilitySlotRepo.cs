using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.Base;

namespace Cliniq.DataAccess.Repos.interfaces
{
    public interface IAvailabilitySlotRepo : IBaseRepo<AvailabilitySlot>
    {
        Task<IReadOnlyList<AvailabilitySlot>> GetAvailableBetweenAsync(
            int doctorId,
            DateTime startAtUtc,
            DateTime endAtUtc);
    }
}
