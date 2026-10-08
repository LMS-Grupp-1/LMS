using Domain.Contracts;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
	private readonly ApplicationDbContext _context = context;
		
	private readonly Lazy<ICourseRepository> _courses = new(() => new CourseRepository(context));

	public ICourseRepository Courses => _courses.Value;

	public async Task CompleteAsync() => await _context.SaveChangesAsync();
}
