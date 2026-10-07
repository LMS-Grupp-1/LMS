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
        if (courseCreateDto.StartDate > courseCreateDto.EndDate)
        {
            throw new ArgumentException("Start date cannot be after end date.");
        }

        var course = new Course
        {
            Name = courseCreateDto.Name,
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
        if (courseUpdateDto.StartDate > courseUpdateDto.EndDate)
        {
            throw new ArgumentException("Start date cannot be after end date.");
        }

        var course = await _repository.GetByIdAsync(id, trackChanges: true);
        if (course is null)
        {
            throw new KeyNotFoundException($"Course with ID {id} was not found.");
        }

        course.Name = courseUpdateDto.Name;
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
            throw new KeyNotFoundException($"Course with id {id} was not found.");

        _repository.DeleteCourse(course);
        await _repository.SaveAsync();
    }
}