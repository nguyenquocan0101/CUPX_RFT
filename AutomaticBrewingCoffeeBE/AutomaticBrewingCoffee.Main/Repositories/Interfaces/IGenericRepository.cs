using System.Linq.Expressions;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.EntityFrameworkCore.Query;

namespace AutomaticBrewingCoffee.Repository.Interfaces;

public interface IGenericRepository<T> : IDisposable where T : class
{
    #region Get Async

    Task<T?> SingleOrDefaultAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool ignoreQueryFilter = false);

    Task<TResult?> SingleOrDefaultAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool ignoreQueryFilter = false);

    Task<ICollection<T>> GetListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool ignoreQueryFilter = false);

    Task<ICollection<TResult>> GetListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool ignoreQueryFilter = false
    );

    Task<IPaginate<T>> GetPagingListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        int page = 1,
        int size = 10,
        bool ignoreQueryFilter = false,
        bool ignorePaging = false);

    Task<IPaginate<TResult>> GetPagingListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        int page = 1,
        int size = 10,
        bool ignoreQueryFilter = false);

    #endregion

    #region Insert

    Task InsertAsync(T entity);

    Task InsertRangeAsync(IEnumerable<T> entities);

    #endregion

    #region Update

    void Update(T entity);

    void UpdateRange(IEnumerable<T> entities);

    #endregion

    #region Delete

    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);

    #endregion

    Expression<Func<T, bool>>? BuildSearchPredicate(string? searchKey, string? searchProperty);
    Expression<Func<T, bool>>? BuildDateRangePredicate(DateTime? startDate, DateTime? endDate);
    Func<IQueryable<T>, IOrderedQueryable<T>>? BuildSortingQuery(string? sortProperty, bool ascending = true);
}