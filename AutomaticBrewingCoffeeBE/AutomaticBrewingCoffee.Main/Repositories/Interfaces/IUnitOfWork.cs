using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Repository.Interfaces;

public interface IUnitOfWork : IGenericRepositoryFactory, IDisposable
{
    int Commit();
    Task<int> CommitAsync();

    void ClearTracking();
}

public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    TContext Context { get; }
}