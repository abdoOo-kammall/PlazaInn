using System.Linq.Expressions;
using PlazaCore.Entites;

namespace PlazaCore.Specification
{
    /// <summary>
    /// Reads at most one matching row so callers can test whether a reference exists.
    /// </summary>
    public class ExistsSpecification<T> : Specification<T> where T : BaseEntity
    {
        public ExistsSpecification(Expression<Func<T, bool>> criteria) : base(criteria)
        {
            ApplyPagination(0, 1);
        }
    }
}
