using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Repos.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.DataAccess.Repos
{
    public class AvailabilitySlotRepo : BaseRepo<AvailabilitySlot>, IAvailabilitySlotRepo
    {
        public AvailabilitySlotRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<AvailabilitySlot>> GetAvailableBetweenAsync(
            int doctorId,
            DateTime startAtUtc,
            DateTime endAtUtc)
        {
            if (startAtUtc >= endAtUtc)
            {
                throw new ArgumentException("The end time must be later than the start time.");
            }

            return await Table.AsNoTracking()
                .Where(slot =>
                    slot.DoctorId == doctorId &&
                    slot.IsAvailable &&
                    slot.StartAtUtc < endAtUtc &&
                    startAtUtc < slot.EndAtUtc)
                .OrderBy(slot => slot.StartAtUtc)
                .ToListAsync();
        }
    }
}
