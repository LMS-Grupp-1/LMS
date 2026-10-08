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

			_uow.Courses.CreateCourse(course);
			await _uow.CompleteAsync();

			return new CourseDto(course.Id, course.Name, course.Description, course.StartDate, course.EndDate);
		}

		public async Task UpdateCourseAsync(int id, CourseUpdateDto courseUpdateDto)
		{
			var course = await _uow.Courses.GetByIdAsync(id, trackChanges: true);
			if (course is null)
			{
				throw new KeyNotFoundException($"Course with ID {id} was not found.");
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

			await _uow.CompleteAsync();
		}

		public async Task DeleteCourseAsync(int id)
		{
			var course = await _uow.Courses.GetByIdAsync(id, trackChanges: true);
			if (course is null)
				throw new KeyNotFoundException($"Course with id {id} was not found.");

			_uow.Courses.DeleteCourse(course);
			await _uow.CompleteAsync();
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
					"Course name must contain 1–50 characters.");
			}

			var today = DateTime.Today;

			if (startDate.Date < today || endDate.Date < today)
			{
				throw new ArgumentException(
					"Course dates cannot be before today.");
			}

			if (startDate > endDate)
			{
				throw new ArgumentException(
					"Start date cannot be after end date.");
			}

			var existingCourses =
				await _uow.Courses.GetAllAsync(trackChanges: false);

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
