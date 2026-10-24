using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PlazaCore.Entites;
using PlazaCore.Specification;

namespace PlazaCore.RepositoryContract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        // ---------- Read ----------
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();

        // Using Specification
        Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecification<T> spec);
        Task<T?> GetByIdWithSpecAsync(ISpecification<T> spec);

        // For pagination (count total matching items)
        //Task<int> CountAsync(ISpecification<T> spec);

        // ---------- Write ----------
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> SaveChangesAsync();

        // ---------- Transactions ----------
        /// <summary>
        /// Runs <paramref name="operation"/> inside a database transaction using the configured retry strategy.
        /// The operation may be re-executed on transient failures (e.g. deadlocks), so it must re-read any state it depends on.
        /// </summary>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    }
}
