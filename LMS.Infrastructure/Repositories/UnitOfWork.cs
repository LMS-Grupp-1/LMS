using Domain.Contracts;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IUserRepository Users { get; }
    public ICourseRepository Courses { get; }
    public IModuleRepository Modules { get; }
    public IActivityRepository Activities { get; }
    public IDocumentRepository Documents { get; }
	public IAssignmentRepository Assignments { get; }
	public ISubmissionRepository Submissions { get; }

    public IFeedbackRepository Feedbacks { get; }

    public INotificationRepository Notifications { get; }

    public UnitOfWork(ApplicationDbContext context,
                IUserRepository userRepository,
                      ICourseRepository courseRepository,
                      IModuleRepository moduleRepository,
                      IActivityRepository activityRepository,
                      IDocumentRepository documentRepository,
                      IAssignmentRepository assignmentRepository,
                      ISubmissionRepository submissionRepository,
                      IFeedbackRepository feedbackRepository,
                      INotificationRepository notificationRepository)
    {
        _context = context;
        Users = userRepository;
        Courses = courseRepository;
        Modules = moduleRepository;
        Activities = activityRepository;
        Documents = documentRepository;
        Assignments = assignmentRepository;
		Submissions = submissionRepository;
        Feedbacks = feedbackRepository;
        Notifications = notificationRepository;
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

	public async Task ExecuteInTransactionAsync(Func<Task> action)
	{
		// EnableRetryOnFailure (Program.cs) does not allow a transaction started directly
		// with BeginTransactionAsync. The execution strategy runs the whole block,
		// and retries all of it, transaction included, on a transient failure.
		var strategy = _context.Database.CreateExecutionStrategy();

		await strategy.ExecuteAsync(async () =>
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();

			await action();

			// Only reached when the action did not throw. If it throws, the transaction
			// is disposed without commit, which rolls back every change made inside it.
			await transaction.CommitAsync();
		});
	}

	public void Dispose()
    {
        _context.Dispose();
    }
}
