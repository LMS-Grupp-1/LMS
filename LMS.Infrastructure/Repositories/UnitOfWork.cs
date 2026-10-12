using Domain.Contracts;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public ICourseRepository Courses { get; }
    public IModuleRepository Modules { get; }

    public UnitOfWork(ApplicationDbContext context,
                      ICourseRepository courseRepository,
                      IModuleRepository moduleRepository)
    {
        _context = context;
        Courses = courseRepository;
        Modules = moduleRepository;
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
