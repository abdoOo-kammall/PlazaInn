using System.Collections.Generic;
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

    }
}
