using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Services.DataServices.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface ISerilogEntryDataService : IReadOnlyBaseDataService<SerilogEntry>
    {
        Task<IReadOnlyList<SerilogEntry>> QueryAsync(
            string? level,
            DateTime fromUtc,
            DateTime toUtc,
            string? correlationId,
            int pageSize,
            int pageNumber);

        Task<IReadOnlyList<SerilogEntry>> GetByCorrelationIdAsync(string correlationId);

        Task<int> PurgeOlderThanAsync(TimeSpan retention);

        Task<IReadOnlyDictionary<string, int>> GetErrorCountsAsync(TimeSpan window);
    }
}
