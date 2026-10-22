using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.Constants;
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

        public async Task<IEnumerable<CourseDto>> GetCoursesWithCountsAsync()
        {
            var courseDtos = await _uow.Courses.GetAllWithCountsAsync(trackChanges: false);

            if (courseDtos == null || !courseDtos.Any())
            {
                return Enumerable.Empty<CourseDto>();
            }

            return courseDtos;
        }

        public async Task<IEnumerable<CourseLookupDto>> GetCourseLookupsAsync()
        {
            var courseLookupDtos = await _uow.Courses.GetCourseLookupsAsync(trackChanges: false);

            if (courseLookupDtos == null || !courseLookupDtos.Any())
            {
                return Enumerable.Empty<CourseLookupDto>();
            }

            return courseLookupDtos;
        }

        public async Task<CourseDto> GetCourseByIdAsync(int id)
		{
			var course = await _uow.Courses.GetByIdAsync(id, trackChanges: false);

			if (course is null)
			{
				throw new CourseNotFoundException(id);
			}

			return new CourseDto(
				course.Id, 
				course.Name, 
				course.Description, 
				course.StartDate, 
				course.EndDate);
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
				throw new CourseNotFoundException(id);
			}

			await ValidateCourseAsync(
			courseUpdateDto.Name,
			courseUpdateDto.StartDate,
			courseUpdateDto.EndDate,
			id);

			course.Name = courseUpdateDto.Name.Trim();
			course.Description = courseUpdateDto.Description ?? string.Empty;
			course.StartDate = courseUpdateDto.StartDate;
			course.EndDate = courseUpdateDto.EndDate;

			await _uow.CompleteAsync();
		}

		public async Task DeleteCourseAsync(int id)
		{
			var course = await _uow.Courses.GetByIdAsync(id, trackChanges: true);

            if (course is null)
			{
                throw new CourseNotFoundException(id);
            }     

            _uow.Courses.DeleteCourse(course);
			await _uow.CompleteAsync();
		}

		private async Task ValidateCourseAsync(
			string name,
			DateTime startDate,
			DateTime endDate,
			int? excludedId = null)
		{
			if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > CourseConstraints.NameMaxLength)
			{
				throw new CourseValidationException(
					$"Course name must contain between 1 and " +
					$"{CourseConstraints.NameMaxLength} characters.");
			}

			if (startDate > endDate)
			{
				throw new CourseValidationException(
					"End date can't be before the start date.");
			}

			bool nameExists = await _uow.Courses.NameExistsAsync(
				name, excludedId);

			if (nameExists)
			{
				throw new CourseNameAlreadyExistsException();
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
