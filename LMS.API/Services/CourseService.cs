using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.CourseDtos;

namespace LMS.API.Services;

public class CourseService(ICourseRepository repository) : ICourseService
{
    private readonly ICourseRepository _repository = repository;

    public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await _repository.GetAllAsync(trackChanges: false);
        return courses.Select(c => new CourseDto(c.Id, c.Name, c.Description, c.StartDate, c.EndDate));
    }

    public async Task<CourseDto?> GetCourseByIdAsync(int id)
    {
        var course = await _repository.GetByIdAsync(id, trackChanges: false);
        if (course is null) return null;

        return new CourseDto(course.Id, course.Name, course.Description, course.StartDate, course.EndDate);
    }

    public async Task<CourseDto> CreateCourseAsync(CourseCreateDto courseCreateDto)
    {
        await ValidateCourseAsync(
            courseCreateDto.Name,
            courseCreateDto.StartDate,
            courseCreateDto.EndDate);

        var course = new Course
        {
            Name = courseCreateDto.Name.Trim(),
            Description = courseCreateDto.Description,
            StartDate = courseCreateDto.StartDate,
            EndDate = courseCreateDto.EndDate
        };

        _repository.CreateCourse(course);
        await _repository.SaveAsync();

        return new CourseDto(course.Id, course.Name, course.Description, course.StartDate, course.EndDate);
    }

    public async Task UpdateCourseAsync(int id, CourseUpdateDto courseUpdateDto)
    {
        var course = await _repository.GetByIdAsync(id, trackChanges: true);
        if (course is null)
        {
            throw new KeyNotFoundException($"Course with {id} could not be found.");
        }

        await ValidateCourseAsync(
            courseUpdateDto.Name,
            courseUpdateDto.StartDate,
            courseUpdateDto.EndDate,
            id);

        course.Name = courseUpdateDto.Name.Trim();

        course.Description = courseUpdateDto.Description;
        course.StartDate = courseUpdateDto.StartDate;
        course.EndDate = courseUpdateDto.EndDate;

        _repository.UpdateCourse(course);
        await _repository.SaveAsync();
    }

    public async Task DeleteCourseAsync(int id)
    {
        var course = await _repository.GetByIdAsync(id, trackChanges: true);
        if (course is null)
            throw new KeyNotFoundException($"Course with {id} could not be found.");

        _repository.DeleteCourse(course);
        await _repository.SaveAsync();
    }

    private async Task ValidateCourseAsync(
    string name,
    DateTime startDate,
    DateTime endDate,
    int? excludedId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Course name needs to contain 1-50 characters.");
        }

        var today = DateTime.Today;

        if (startDate.Date < today || endDate.Date < today)
        {
            throw new ArgumentException(
                "Course dates can't be before today's date.");
        }

        if (startDate > endDate)
        {
            throw new ArgumentException(
                "End date can't be before the start date.");
        }

        var existingCourses =
            await _repository.GetAllAsync(trackChanges: false);

        if (existingCourses.Any(course =>
            course.Id != excludedId &&
            string.Equals(
                course.Name.Trim(),
                name.Trim(),
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                "A course with this name already exists.");
        }
    }

}