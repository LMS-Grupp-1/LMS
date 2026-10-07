namespace Domain.Contracts;

public interface IUnitOfWork
{
	ICourseRepository Courses { get; }
		
	Task CompleteAsync();
}