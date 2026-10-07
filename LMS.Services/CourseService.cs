using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.CourseDtos;
using Service.Contracts;

namespace LMS.Services
{
	public class CourseService(IUnitOfWork uow) : ICourseService
	{
		private readonly IUnitOfWork _uow = uow;


		public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
		{
			var courses = await _uow.Courses.GetAllAsync(trackChanges: false);
			return courses.Select(c => new CourseDto(c.Id, c.Name, c.Description, c.StartDate, c.EndDate));
		}

		public async Task<CourseDto?> GetCourseByIdAsync(int id)
		{
			var course = await _uow.Courses.GetByIdAsync(id, trackChanges: false);
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

			_uow.Courses.CreateCourse(course);
			await _uow.CompleteAsync();

			return new CourseDto(course.Id, course.Name, course.Description, course.StartDate, course.EndDate);
		}

		public async Task UpdateCourseAsync(int id, CourseUpdateDto courseUpdateDto)
		{
			if (courseUpdateDto.StartDate > courseUpdateDto.EndDate)
			{
				throw new ArgumentException("Start date cannot be after end date.");
			}

			var course = await _uow.Courses.GetByIdAsync(id, trackChanges: true);
			if (course is null)
			{
				throw new KeyNotFoundException($"Course with ID {id} was not found.");
			}

			course.Name = courseUpdateDto.Name;
			course.Description = courseUpdateDto.Description;
			course.StartDate = courseUpdateDto.StartDate;
			course.EndDate = courseUpdateDto.EndDate;
						
			await _uow.CompleteAsync();
		}

		public async Task<StudentCourseDto> GetMyCourseAsync(string userId)
		{			
			var course = await _uow.Courses.GetForStudentAsync(userId, trackChanges: false)
				?? throw new StudentCourseNotFoundException();

			return new StudentCourseDto(
				course.Id,
				course.Name,
				course.Description,
				course.StartDate,
				course.EndDate,
				course.Students
					.Select(s => new ParticipantDto(s.Id, s.FirstName, s.LastName))
					.ToList());
		}
	}
}
