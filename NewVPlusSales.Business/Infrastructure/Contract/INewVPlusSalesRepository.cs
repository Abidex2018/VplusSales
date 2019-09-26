using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace NewVPlusSales.Business.Infrastructure.Contract
{
    public enum SortOrder { Ascending, Descending }
    internal interface INewVPlusSalesRepository<T> where T : class
    {
        DbContext RepositoryContext();
        T Add(T entity);
        IEnumerable<T> AddRange(List<T> entities);
        T Remove(T entity);
        T Remove(object key);
        T Update(T entity);
        IEnumerable<T> UpdateRange(List<T> entities);
        IQueryable<T> GetAll();

        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetAll(string includeProperties);
        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate, string includeProperties);
        T GetById(object key);
        IEnumerable<T> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "");
        IEnumerable<T> Get<TOrderBy>(Expression<Func<T, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending);
        IEnumerable<T> Get<TOrderBy>(Expression<Func<T, bool>> criteria, Expression<Func<T, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending, string includeProperties = "");
    }
}
