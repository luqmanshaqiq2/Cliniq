using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.DataAccess.Repos.Base
{
    public abstract class BaseRepo<T> : BaseViewRepo<T>, IBaseRepo<T> where T : BaseEntity, new()
    {
        protected BaseRepo(ApplicationDbContext context) : base(context) { }
        

        public virtual async Task<T?> FindAsync(int id)
            => await Table.FirstOrDefaultAsync(x => x.Id == id);

        public virtual async Task<T?> FindAsNoTrackingAsync(int id) //untracked by efcore and faster
            => await Table.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(x => x.Id == id);

        public virtual async Task<T?> FindIgnoreQueryFiltersAsync(int id)//finds by Id even if a query filter would hide it
            => await Table.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);

        public virtual void ExecuteParameterizedQuery(string sql, object[] sqlParametersObjects)//safe against SQL Injection - synchronus and returns nothing - reference
            => Context.Database.ExecuteSqlRaw(sql, sqlParametersObjects);

        public virtual async Task<int> AddAsync(T entity, bool persist = true)
        {
            await Table.AddAsync(entity);
            return persist ? await SaveChangesAsync() : 0;
        }

        public virtual async Task<int> AddRangeAsync(IEnumerable<T> entities, bool persist = true)//adds many entities
        {
            await Table.AddRangeAsync(entities);
            return persist ? await SaveChangesAsync() : 0;
        }

        public virtual async Task<int> UpdateAsync(T entity, bool persist = true)
        {
            Table.Update(entity);
            Context.ChangeTracker.DetectChanges();//marks entity modified and calls to detect changes
            return persist ? await SaveChangesAsync() : 0;//save changes permenently or not
        }

        public virtual async Task<int> UpdateRangeAsync(IEnumerable<T> entities, bool persist = true)
        {
            Table.UpdateRange(entities);
            return persist ? await SaveChangesAsync() : 0;
        }

        public async Task<int> DeleteAsync(int id, byte[] timeStamp, bool persist = true)
        {
            //*deletes without loading the db using a stub entity(id,timestamp) and marks it deleted.
            var entity = new T { Id = id, TimeStamp = timeStamp };
            Context.Entry(entity).State = EntityState.Deleted;
            return persist ? await SaveChangesAsync() : 0;
        }

        public virtual async Task<int> DeleteAsync(T entity, bool persist = true)
        {
            Table.Remove(entity);
            return persist ? await SaveChangesAsync() : 0;
        }

        public virtual async Task<int> DeleteRangeAsync(IEnumerable<T> entities, bool persist = true)
        {
            Table.RemoveRange(entities);
            return persist ? await SaveChangesAsync() : 0;
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {

            return await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {

                throw new DbUpdateConcurrencyException("Record has been modified or deleted by another user", ex);
            }
            catch (Exception ex)
            {
                throw new UnknownDatabaseException("Fallback Database Error, report unknown error", ex);
            }

        }

       
    }
}