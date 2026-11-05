using System.Linq;
using System.Linq.Expressions;
using System.Globalization;
using System.Reflection;

namespace Cliniq.DataAccess.Helpers
{
    public static class LinqHelpers
    {
        public static Expression<Func<T, bool>> BuildWherePredicate<T>(
            PropertyInfo propertyInfo,
            string filterQuery)
        {
            return BuildWherePredicate<T>(propertyInfo.Name, filterQuery);
        }

        public static Expression<Func<T, bool>> BuildWherePredicateForNestedProperty<T>(
            string propertyPath,
            string filterQuery)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            var properties = propertyPath.Split('.');
            Expression propertyAccess = parameter;
            Expression? nullCheck = null;

            foreach (var propertyName in properties)
            {
                var propertyInfo = propertyAccess.Type.GetProperty(
                    propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                    ?? throw new ArgumentException(
                        $"Property '{propertyName}' was not found in '{propertyAccess.Type.Name}'.",
                        nameof(propertyPath));

                if (!propertyAccess.Type.IsValueType)
                {
                    var check = Expression.NotEqual(
                        propertyAccess,
                        Expression.Constant(null, propertyAccess.Type));
                    nullCheck = nullCheck is null
                        ? check
                        : Expression.AndAlso(nullCheck, check);
                }

                propertyAccess = Expression.Property(propertyAccess, propertyInfo);
            }

            var filter = BuildFilterExpression(propertyAccess, filterQuery);
            var body = nullCheck is null ? filter : Expression.AndAlso(nullCheck, filter);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static LambdaExpression BuildOrderByFunction<T>(PropertyInfo propertyInfo)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            var property = Expression.Property(parameter, propertyInfo);
            return Expression.Lambda(property, parameter);
        }

        public static IQueryable<T> OrderByProperty<T>(
            IQueryable<T> query,
            LambdaExpression propertySelector,
            bool ascending)
        {
            var methodName = ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);
            var method = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(candidate =>
                    candidate.Name == methodName
                    && candidate.IsGenericMethodDefinition
                    && candidate.GetGenericArguments().Length == 2
                    && candidate.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertySelector.ReturnType);
            var orderedQuery = Expression.Call(
                method,
                query.Expression,
                Expression.Quote(propertySelector));

            return query.Provider.CreateQuery<T>(orderedQuery);
        }

        private static Expression<Func<T, bool>> BuildWherePredicate<T>(
            string propertyPath,
            string filterQuery)
        {
            return BuildWherePredicateForNestedProperty<T>(propertyPath, filterQuery);
        }

        private static Expression BuildFilterExpression(Expression property, string filterQuery)
        {
            var propertyType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;

            if (propertyType == typeof(string))
            {
                var value = Expression.Constant(filterQuery);
                return Expression.AndAlso(
                    Expression.NotEqual(property, Expression.Constant(null, property.Type)),
                    Expression.Call(property, nameof(string.Contains), Type.EmptyTypes, value));
            }

            var parsedValue = propertyType.IsEnum
                ? Enum.Parse(propertyType, filterQuery, ignoreCase: true)
                : propertyType == typeof(Guid)
                    ? Guid.Parse(filterQuery)
                    : Convert.ChangeType(filterQuery, propertyType, CultureInfo.InvariantCulture);
            var constant = Expression.Constant(parsedValue, propertyType);
            Expression comparisonValue = property.Type == propertyType
                ? constant
                : Expression.Convert(constant, property.Type);

            return Expression.Equal(property, comparisonValue);
        }

        public static IQueryable<T> Search<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> predicate)
        {
            return query.Where(predicate);
        }

        public static IQueryable<T> FilterByDate<T>(
            this IQueryable<T> query,
            Expression<Func<T, DateTime>> dateSelector,
            DateTime? from,
            DateTime? to)
        {
            if (from.HasValue)
            {
                var start = Expression.GreaterThanOrEqual(
                    dateSelector.Body,
                    Expression.Constant(from.Value));

                query = query.Where(
                    Expression.Lambda<Func<T, bool>>(
                        start, dateSelector.Parameters));
            }

            if (to.HasValue)
            {
                var end = Expression.LessThanOrEqual(
                    dateSelector.Body,
                    Expression.Constant(to.Value));

                query = query.Where(
                    Expression.Lambda<Func<T, bool>>(
                        end, dateSelector.Parameters));
            }

            return query;
        }

        public static IQueryable<T> SortBy<T, TKey>(
            this IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            bool descending = false)
        {
            return descending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }

        public static IQueryable<T> Paginate<T>(
            this IQueryable<T> query,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
    }
}