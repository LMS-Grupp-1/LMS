using Domain.Contracts;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IModuleRepository Modules { get; }

    public UnitOfWork(ApplicationDbContext context,
                      IModuleRepository moduleRepository)
    {
        _context = context;
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
