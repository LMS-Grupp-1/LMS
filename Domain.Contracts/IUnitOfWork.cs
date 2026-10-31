namespace Domain.Contracts;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICourseRepository Courses { get; }
    IModuleRepository Modules { get; }
    IActivityRepository Activities { get; }
    IDocumentRepository Documents { get; }
	IAssignmentRepository Assignments { get; }
	ISubmissionRepository Submissions { get; }
    IFeedbackRepository Feedbacks { get; }
    INotificationRepository Notifications { get; }

    ISharedDocumentRepository SharedDocuments { get; }

    Task<int> CompleteAsync(); // Commits all changes to the database
}