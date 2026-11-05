using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.DataAccess.Repos.Base
{
//

    public abstract class BaseViewRepo<T> : IBaseViewRepo<T> where T : class, new()
    {
        private bool disposedValue;

        public DbSet<T> Table { get; }

        public ApplicationDbContext Context { get; }

        protected BaseViewRepo(ApplicationDbContext context)
        {
            Context = context;
            Table = Context.Set<T>();

        }


        public virtual IEnumerable<T> ExecuteSqlString(string sqlString)
        {
            return Table.FromSqlRaw(sqlString); //maps the sql string to T. 
            //if sql user input is a string then its open to sql injection(bad)
            //safety measure : parameterized queries(good)
        }

        public virtual IEnumerable<T> GetAll()
        {
            return Table.AsQueryable(); //returns as queryable(nothing executes until we request results)
        }


        public virtual IEnumerable<T> GetAllIgnoreQueryFilters(
            string? filterOn = null, string? filterQuery = null, //if filter is on and query filter given 
            string? sortBy = null, bool isAscending = true, //sortBy prop is valid then orders else silently ignored
            int pageSize = 10, int pageNumber = 1
            )
            //
        {

            var table = Table.IgnoreQueryFilters().AsQueryable();

            // Filtering
            if (!String.IsNullOrWhiteSpace(filterOn) && !String.IsNullOrWhiteSpace(filterQuery))
            {

                // Get the PropertyInfo object for the property to filter on using reflection(inspecting a type at runtime to discover properties)
                var propertyInfo = typeof(T).GetProperty(filterOn, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    // Construct the predicate(return true or false) dynamically
                    var predicate = LinqHelpers.BuildWherePredicate<T>(propertyInfo, filterQuery);

                    // Apply the predicate to filter the query
                    table = table.Where(predicate);
                }

                // Else check if the filter contains a dot(Doctor.Name)
                else if (filterOn.Contains("."))
                {
                    // This time, build the predicate dynamically using System.Linq.Dynamic.Core Package
                    var predicate = LinqHelpers.BuildWherePredicateForNestedProperty<T>(filterOn, filterQuery);
                    table = table.Where(predicate);

                }
                else
                {

                    throw new ArgumentException("Invalid filterOn property");
                }
            }


            // Sorting
            if (!String.IsNullOrWhiteSpace(sortBy))
            {
                var propertyInfo = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {

                    var function = LinqHelpers.BuildOrderByFunction<T>(propertyInfo);

                    table = LinqHelpers.OrderByProperty(table, function, isAscending);
                }


            }

            // Pagination
            int toSkip = (pageNumber - 1) * pageSize;
            table = table.Skip(toSkip).Take(pageSize);


            return table.ToList();
        }

       
    //disposes dbContext when repository is disposed.
        public void Dispose()
        {
            Context.Dispose();
        }
    }
}