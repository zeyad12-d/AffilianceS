using System.Linq.Expressions;

namespace Affiliance_core.interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);

      
        Task<IReadOnlyList<T>> GetAllAsync();

      
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            string[]? includes = null);

        
        Task<IEnumerable<T>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string[]? includes = null);
        
        IQueryable<T> GetQueryable();

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}