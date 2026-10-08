namespace Domain.Contracts;

public interface IUnitOfWork : IDisposable
{
    IModuleRepository Modules { get; }
    Task<int> CompleteAsync(); // Commits all changes to the database
}