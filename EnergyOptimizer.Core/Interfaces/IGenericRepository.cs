using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Generic repository pattern for CRUD operations

        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<T> GetEntityWithSpec(ISpecification<T> spec);
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task<bool> AnyAsync(ISpecification<T> spec);

        Task<T> AddAsync(T entity);
        // Add multiple entities
        Task AddRangeAsync(IEnumerable<T> entities);

        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);

        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task<int> SaveChangesAsync();

    }
}
