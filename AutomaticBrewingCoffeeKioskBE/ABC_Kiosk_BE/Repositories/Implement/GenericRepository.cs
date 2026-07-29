using System.Linq.Expressions;
using Domain.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Repositories.Interfaces;

namespace Repositories.Implement
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        #region Gett Async

        public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (orderBy != null) query = orderBy(query);

            return await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public virtual async Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (orderBy != null) query = orderBy(query);

            return await query.AsNoTracking().Select(selector).FirstOrDefaultAsync();
        }

        public virtual async Task<ICollection<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (orderBy != null) query = orderBy(query);

            return await query.AsNoTracking().ToListAsync();
        }

        public virtual async Task<ICollection<TResult>> GetListAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (orderBy != null) return await orderBy(query).AsNoTracking().Select(selector).ToListAsync();

            return await query.Select(selector).ToListAsync();
        }

        public Task<IPaginate<T>> GetPagingListAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, int page = 1,
            int size = 10)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return orderBy(query).ToPaginateAsync(page, size, 1);
            return query.AsNoTracking().ToPaginateAsync(page, size, 1);
        }

        public Task<IPaginate<TResult>> GetPagingListAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, int page = 1, int size = 10)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return orderBy(query).Select(selector).ToPaginateAsync(page, size, 1);
            return query.AsNoTracking().Select(selector).ToPaginateAsync(page, size, 1);
        }

        #endregion

        #region Insert

        public async Task InsertAsync(T entity)
        {
            if (entity == null) return;
            await _dbSet.AddAsync(entity);
        }

        public async Task InsertRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        #endregion

        #region Update
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        #endregion

        public Expression<Func<T, bool>>? BuildSearchPredicate(dynamic? searchKey, dynamic? searchProperty)
        {
            if (string.IsNullOrWhiteSpace(searchKey) || string.IsNullOrWhiteSpace(searchProperty))
                return null;

            // find T property with ignore case
            var propertyInfo = typeof(T).GetProperties()
                                        .FirstOrDefault(p => string.Equals(p.Name, searchProperty, StringComparison.OrdinalIgnoreCase));

            if (propertyInfo == null)
                throw new InvalidOperationException($"Property '{searchProperty}' does not exist on type '{typeof(T).Name}'.");

            // create Expression for predicate
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo.Name);

            if (property.Type != typeof(string))
                throw new InvalidOperationException("Search property must be a string.");

            var searchValue = Expression.Constant(searchKey);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsExpression = Expression.Call(property, containsMethod!, searchValue);

            return Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
        }
        public Func<IQueryable<T>, IOrderedQueryable<T>>? BuildSortingQuery(string? sortProperty, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(sortProperty))
                return null;

            // Find the property on type T with case-insensitive comparison
            var propertyInfo = typeof(T).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, sortProperty, StringComparison.OrdinalIgnoreCase));

            // Create the parameter and property expression
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo.Name);

            // Build the lambda expression
            var keySelector = Expression.Lambda(property, parameter);

            // Determine method name (OrderBy or OrderByDescending)
            var methodName = ascending ? "OrderBy" : "OrderByDescending";

            return query =>
            {
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(T), propertyInfo.PropertyType },
                    query.Expression,
                    Expression.Quote(keySelector)
                );

                return (IOrderedQueryable<T>)query.Provider.CreateQuery(resultExpression);
            };
        }
    }
}
