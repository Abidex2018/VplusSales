using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using NewVPlusSales.Business.Infrastructure.Contract;

namespace NewVPlusSales.Business.Infrastructure
{
    internal class NewVPlusSalesRepository<T> : INewVPlusSalesRepository<T> where T : class
    {
         private readonly INewVPlusSalesUoWork _context;
		 private readonly DbSet<T> _dbSet;
		 private readonly DbContext _dbContext;

         public NewVPlusSalesRepository(INewVPlusSalesUoWork uoWork)
		{
		    _context = uoWork ?? throw new ArgumentNullException(nameof(uoWork));
			 _dbSet = uoWork.Context.NewVPlusSalesDbContext.Set<T>();
			 _dbContext = uoWork.Context.NewVPlusSalesDbContext;
		}

		public T Add(T entity)
		{
			 return _dbSet.Add(entity);
		}

        public IEnumerable<T> AddRange(List<T> entities)
        {
            return _dbSet.AddRange(entities);
        }
		public T Remove(T entity)
		{
			  return _dbSet.Remove(entity);
		}

		public T Remove(object key)
		{
			 var entity = _dbSet.Find(key);
			 return _dbSet.Remove(entity);
		}

		public T Update(T entity)
		{

            var updated = _dbSet.Attach(entity);
            _context.Context.NewVPlusSalesDbContext.Entry(entity).State = EntityState.Modified;
            return updated;
            
		}

        public IEnumerable<T> UpdateRange(List<T> entities)
        {
            var retVals = new List<T>();
            foreach (var item in entities)
            {
                var updated = _dbSet.Attach(item);
                _context.Context.NewVPlusSalesDbContext.Entry(item).State = EntityState.Modified;
                retVals.Add(updated);
            }
            return retVals;
        }

		public IQueryable<T> GetAll()
		{
            return _dbSet.AsNoTracking();
		}

		public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
		{
			 return GetAll().Where(predicate);
		}
        public IQueryable<T> GetAll(string includeProperties)
        {
            var query = GetAll();
            if (!query.Any()) { return null; }
            if (string.IsNullOrEmpty(includeProperties)) { return query; }
            return includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

		public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate, string includeProperties)
		{
			 var query = GetAll().Where(predicate);
			 if (!query.Any()) {return null;}
			 if (string.IsNullOrEmpty(includeProperties)){return query;}
			  return includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
		}

		public  T GetById(object key)
		{
            return _dbSet.Find(key);
		}

		public IEnumerable<T> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
		{
            IQueryable<T> query = _dbSet.AsNoTracking();
			 if (filter != null)
			 {
				 query = query.Where(filter);
			 }
			 query = includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
			 return orderBy != null ? orderBy(query).ToList() : query.ToList();
		}

		public IEnumerable<T> Get<TOrderBy>(Expression<Func<T, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
		{
            return sortOrder == SortOrder.Ascending ? GetAll().OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable() : GetAll().OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
		}

		public IEnumerable<T> Get<TOrderBy>(Expression<Func<T, bool>> criteria, Expression<Func<T, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending, string includeProperties = "")
		{
            var filtValue = GetAll(criteria, includeProperties);
			if (filtValue == null){return null;}
			return sortOrder == SortOrder.Ascending ? filtValue.OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable() : filtValue.OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
		}

		public DbContext RepositoryContext()
		{
			 return _dbContext;
		}
    }
}
