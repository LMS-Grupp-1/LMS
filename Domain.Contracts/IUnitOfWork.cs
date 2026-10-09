namespace Domain.Contracts;

public interface IUnitOfWork : IDisposable
{
    ICourseRepository Courses { get; }
    IModuleRepository Modules { get; }
    Task<int> CompleteAsync(); // Commits all changes to the database
}