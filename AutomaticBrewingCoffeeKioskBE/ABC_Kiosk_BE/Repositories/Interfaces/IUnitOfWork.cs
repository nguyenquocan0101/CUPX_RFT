using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repositories.Interfaces
{
    public interface IUnitOfWork : IGenericRepositoryFactory, IDisposable
    {
        int Commit();
        Task<int> CommitAsync();

    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        public TContext Context { get; }
        public Task<TContext> GetContextAsync();
    }
}
