namespace Domain.Contracts;

public interface IUnitOfWork : IDisposable
{
    ICourseRepository Courses { get; }
    IModuleRepository Modules { get; }
    IActivityRepository Activities { get; }
    IDocumentRepository Documents { get; }
	IAssignmentRepository Assignments { get; }
	Task<int> CompleteAsync(); // Commits all changes to the database
}