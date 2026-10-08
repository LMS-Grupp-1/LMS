using Domain.Models.Entities;

namespace Domain.Contracts;

public interface ICourseRepository
{
	Task<IEnumerable<Course>> GetAllAsync(bool trackChanges);
	Task<Course?> GetByIdAsync(int id, bool trackChanges);
	Task<Course?> GetForStudentAsync(string userId, bool trackChanges);
	void CreateCourse(Course course);
	void UpdateCourse(Course course);
	void DeleteCourse(Course course);
}
