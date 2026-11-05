using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Cliniq.Models.Entities.Base;

namespace Cliniq.Services.DataServices.Base
{
    public interface IReadOnlyBaseDataService<TEntity>
        where TEntity : BaseEntity, new()
    {
        Task<IEnumerable<TEntity>> GetAllAsync(
            string? filterOn,
            string? filterQuery,
            string? sortBy,
            bool isAscending,
            int pageSize,
            int pageNumber);

        Task<TEntity?> FindAsync(int id);
    }
}
