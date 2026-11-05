using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.Base;
using Cliniq.Services.Logging;

namespace Cliniq.Services.DataServices.Base
{
    public class BaseDataService
    {
        public abstract class BaseDalDataService<TEntity, TDataService>
        where TEntity : BaseEntity, new()
        where TDataService : class

    {
        protected readonly IBaseRepo<TEntity> _mainRepo;
        protected readonly IAppLogging<TDataService> _logger;

        protected BaseDalDataService(IBaseRepo<TEntity> mainRepo, IAppLogging<TDataService> logger)
        {
            _mainRepo = mainRepo;
            _logger = logger;
        }


        public virtual Task<IEnumerable<TEntity>> GetAllAsync(
            string? filterOn, string? filterQuery,
            string? sortBy, bool isAscending,
            int pageSize, int pageNumber)
        {
            try
            {
                var entities = _mainRepo.GetAllIgnoreQueryFilters(filterOn, filterQuery,
                    sortBy, isAscending, pageSize, pageNumber);
                _logger.LogDebug(
                    "Retrieved {EntityType} records with page {PageNumber} and size {PageSize}.",
                    typeof(TEntity).Name, pageNumber, pageSize);
                return Task.FromResult(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve {EntityType} records.", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<TEntity?> FindAsync(int id)
        {
            try
            {
                var entity = await _mainRepo.FindAsync(id);
                if (entity is null)
                {
                    _logger.LogWarning("{EntityType} with ID {EntityId} was not found.",
                        typeof(TEntity).Name, id);
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve {EntityType} with ID {EntityId}.",
                    typeof(TEntity).Name, id);
                throw;
            }
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, bool persist = true)
        {
            try
            {
                await _mainRepo.UpdateAsync(entity, persist);
                _logger.LogInformation("Updated {EntityType} with ID {EntityId}.",
                    typeof(TEntity).Name, entity.Id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update {EntityType} with ID {EntityId}.",
                    typeof(TEntity).Name, entity.Id);
                throw;
            }
        }

        public virtual async Task DeleteAsync(TEntity entity, bool persist = true)
        {
            try
            {
                await _mainRepo.DeleteAsync(entity, persist);
                _logger.LogInformation("Deleted {EntityType} with ID {EntityId}.",
                    typeof(TEntity).Name, entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete {EntityType} with ID {EntityId}.",
                    typeof(TEntity).Name, entity.Id);
                throw;
            }
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, bool persist = true)
        {
            try
            {
                await _mainRepo.AddAsync(entity, persist);
                _logger.LogInformation("Added {EntityType} with ID {EntityId}.",
                    typeof(TEntity).Name, entity.Id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add {EntityType}.", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual void ResetChangeTracker()
        {
            _mainRepo.Context.ChangeTracker.Clear();
        }
    }
    }
}