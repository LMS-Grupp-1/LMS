using Domain.Contracts;
using LMS.Infrastructure.Data;
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

    public void Dispose()
    {
        _context.Dispose();
    }
}
