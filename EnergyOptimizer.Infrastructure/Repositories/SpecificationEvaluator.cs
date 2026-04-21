using EnergyOptimizer.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.Infrastructure.Repositories
{
    public static class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

            // Apply criteria "where" clause
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            // Apply expression-based includes
            query = spec.IncludeStrings
               .Aggregate(query, (current, include) => current.Include(include));

            // Apply string-based includes
            query = spec.Includes
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply ordering
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            // Apply grouping
            if (spec.GroupBy != null)
            {
                query = query.GroupBy(spec.GroupBy).SelectMany(x => x);
            }

            // Apply distinct
            if (spec.IsDistinct)
            {
                query = query.Distinct();
            }

            // Apply paging
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            // Apply AsNoTracking
            if (spec.AsNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
    }
}
