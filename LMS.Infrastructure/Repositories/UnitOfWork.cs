using Domain.Contracts;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public ICourseRepository Courses { get; }
    public IModuleRepository Modules { get; }
    public IActivityRepository Activities { get; }
    public IDocumentRepository Documents { get; }
	public IAssignmentRepository Assignments { get; }
	public ISubmissionRepository Submissions { get; }

	public UnitOfWork(ApplicationDbContext context,
                      ICourseRepository courseRepository,
                      IModuleRepository moduleRepository,
                      IActivityRepository activityRepository,
                      IDocumentRepository documentRepository,
                      IAssignmentRepository assignmentRepository,
                      ISubmissionRepository submissionRepository)
    {
        _context = context;
        Courses = courseRepository;
        Modules = moduleRepository;
        Activities = activityRepository;
        Documents = documentRepository;
        Assignments = assignmentRepository;
		Submissions = submissionRepository;
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
