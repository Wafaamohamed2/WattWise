
namespace EnergyOptimizer.Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Generic repository pattern for CRUD operations

        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<T?> GetEntityWithSpec(ISpecification<T> spec);
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task<bool> AnyAsync(ISpecification<T> spec);

        Task<T> AddAsync(T entity);
        // Add multiple entities
        Task AddRangeAsync(IEnumerable<T> entities);

        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);

        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task<int> SaveChangesAsync();

    }
}
