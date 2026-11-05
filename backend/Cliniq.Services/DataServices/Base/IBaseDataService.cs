using Backend.Cliniq.Models.Entities.Base;

namespace Cliniq.Services.DataServices.Base
{
    public interface IBaseDataService<TEntity> : IReadOnlyBaseDataService<TEntity>
        where TEntity : BaseEntity, new()
    {
        Task<TEntity> UpdateAsync(TEntity entity, bool persist = true);

        Task<TEntity> AddAsync(TEntity entity, bool persist = true);

        Task DeleteAsync(TEntity entity, bool persist = true);

        void ResetChangeTracker();
    }
}
