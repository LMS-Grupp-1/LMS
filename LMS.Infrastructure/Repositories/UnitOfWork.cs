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

    public UnitOfWork(ApplicationDbContext context,
                IUserRepository userRepository,
                      ICourseRepository courseRepository,
                      IModuleRepository moduleRepository,
                      IActivityRepository activityRepository,
                      IDocumentRepository documentRepository)
    {
        _context = context;
        Users = userRepository;
        Courses = courseRepository;
        Modules = moduleRepository;
        Activities = activityRepository;
        Documents = documentRepository;
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
