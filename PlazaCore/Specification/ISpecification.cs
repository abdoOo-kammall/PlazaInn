using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using PlazaCore.Entites;

namespace PlazaCore.Specification
{
    public interface ISpecification<T> where T : BaseEntity
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDesc { get; }
        int Skip { get; }
        int Take { get; }
        bool IsPaginationEnabled { get; }
    }
}
