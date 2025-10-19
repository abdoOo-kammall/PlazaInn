    using Microsoft.EntityFrameworkCore;
    using PlazaCore.Entites;
    using PlazaCore.RepositoryContract;
    using PlazaCore.Specification;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    namespace PlazaRepository
    {
        public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
        {
            private readonly PlazaDbContext _context;
            private readonly DbSet<T> _dbSet;

            public GenericRepository(PlazaDbContext context)
            {
                _context = context;
                _dbSet = _context.Set<T>();
            }

            // ---------- Read ----------

            public async Task<T?> GetByIdAsync(int id)
            {
                return await _dbSet.FindAsync(id);
            }

            public async Task<IEnumerable<T>> GetAllAsync()
            {
                return await _dbSet.ToListAsync();
            }

            public async Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecification<T> spec)
            {
                var query = ApplySpecification(spec);
                return await query.ToListAsync();
            }

            public async Task<T?> GetByIdWithSpecAsync(ISpecification<T> spec)
            {
                var query = ApplySpecification(spec);
                return await query.FirstOrDefaultAsync();
            }

            public async Task<int> CountAsync(ISpecification<T> spec)
            {
                var query = ApplySpecification(spec);
                return await query.CountAsync();
            }

            // ---------- Write ----------

            public async Task AddAsync(T entity)
            {
                await _dbSet.AddAsync(entity);
            }

            public void Update(T entity)
            {
                _dbSet.Update(entity);
            }

            public void Delete(T entity)
            {
                _dbSet.Remove(entity);
            }

            // ---------- Private Helpers ----------

            private IQueryable<T> ApplySpecification(ISpecification<T> spec)
            {
                return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
            }

            public async Task<int> SaveChangesAsync()
            {
                return await _context.SaveChangesAsync();
            }
        }
    }
