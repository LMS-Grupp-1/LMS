using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using Moq;

namespace LMS.Services.Tests
{
	/// <summary>
	/// Unit tests for <see cref="CourseService"/>.
	/// The repository is mocked with Moq, so the tests run without a database
	/// and only verify the logic in the service itself.
	/// </summary>
	/// <remarks>
	/// Not covered here:
	/// - The database query in CourseRepository.GetForStudentAsync (Include, Where, OrderBy),
	///   which requires an integration test against a real database.
	/// - Role-based access on CourseController, which is handled by ASP.NET Core
	///   and requires an integration test with an authenticated user.
	/// </remarks>
	public class CourseServiceTests
	{
		private readonly Mock<ICourseRepository> _courseRepository = new();
		private readonly Mock<IUnitOfWork> _uow = new();
		private readonly CourseService _sut;

		// xUnit creates a new instance of this class for every test,
		// so each test starts with fresh mocks and no shared state.
		public CourseServiceTests()
		{
			// The service reaches the repository through the unit of work
			_uow.Setup(u => u.Courses).Returns(_courseRepository.Object);

			// System under test
			_sut = new CourseService(_uow.Object);
		}

		/// <summary>
		/// A student who is not enrolled in any course must get
		/// <see cref="StudentCourseNotFoundException"/>.
		/// The global exception handler turns it into a 404, which the Blazor
		/// pages show as "You are not enrolled in a course yet."
		/// </summary>
		[Fact]
		public async Task GetMyCourseAsync_StudentWithoutCourse_ThrowsStudentCourseNotFoundException()
		{
			// Arrange: the repository finds no course for the student
			_courseRepository
				.Setup(r => r.GetForStudentAsync("student-1", false))
				.ReturnsAsync((Course?)null);

			// Act & Assert
			await Assert.ThrowsAsync<StudentCourseNotFoundException>(
				() => _sut.GetMyCourseAsync("student-1"));
		}

		/// <summary>
		/// The course fields must be copied unchanged from the entity to the DTO.
		/// Catches swapped or missing fields in the mapping.
		/// </summary>
		[Fact]
		public async Task GetMyCourseAsync_StudentWithCourse_MapsCourseFields()
		{
			// Arrange
			var course = CreateCourse();
			_courseRepository
				.Setup(r => r.GetForStudentAsync("student-1", false))
				.ReturnsAsync(course);

			// Act
			var result = await _sut.GetMyCourseAsync("student-1");

			// Assert
			Assert.Equal(course.Id, result.Id);
			Assert.Equal(course.Name, result.Name);
			Assert.Equal(course.Description, result.Description);
			Assert.Equal(course.StartDate, result.StartDate);
			Assert.Equal(course.EndDate, result.EndDate);
		}

		/// <summary>
		/// All participants must be mapped with id, first name and last name,
		/// in the same order as returned by the repository.
		/// The repository sorts by last name; the service must keep that order.
		/// </summary>
		[Fact]
		public async Task GetMyCourseAsync_StudentWithCourse_MapsParticipantsInSameOrder()
		{
			// Arrange
			_courseRepository
				.Setup(r => r.GetForStudentAsync("student-1", false))
				.ReturnsAsync(CreateCourse());

			// Act
			var result = await _sut.GetMyCourseAsync("student-1");
						
			// Assert: Assert.Collection checks the count, the order and each element
			Assert.Collection(result.Participants,
				p =>
				{
					Assert.Equal("student-1", p.Id);
					Assert.Equal("Anna", p.FirstName);
					Assert.Equal("Andersson", p.LastName);
				},
				p =>
				{
					Assert.Equal("student-2", p.Id);
					Assert.Equal("Erik", p.FirstName);
					Assert.Equal("Eriksson", p.LastName);
				},
				p =>
				{
					Assert.Equal("student-3", p.Id);
					Assert.Equal("Maria", p.FirstName);
					Assert.Equal("Svensson", p.LastName);
				});
		}

		/// <summary>
		/// The service must look up the course with the logged-in user's id,
		/// read-only and exactly once. Passing another id would let a student
		/// see the wrong course, which is a security issue.
		/// </summary>
		[Fact]
		public async Task GetMyCourseAsync_CallsRepositoryWithUserIdAndNoTracking()
		{
			// Arrange: return a course for any arguments, so only Verify decides the outcome
			_courseRepository
				.Setup(r => r.GetForStudentAsync(It.IsAny<string>(), It.IsAny<bool>()))
				.ReturnsAsync(CreateCourse());

			// Act
			await _sut.GetMyCourseAsync("student-1");
						
			// Assert: same user id, trackChanges false, called exactly once
			_courseRepository.Verify(
				r => r.GetForStudentAsync("student-1", false),
				Times.Once);
		}

		/// <summary>
		/// Creates a course with three students, already sorted by last name
		/// as the repository returns them.
		/// </summary>		
		private static Course CreateCourse() => new()
		{
			Id = 1,
			Name = "Lexicon LTU",
			Description = "Fullstack .NET developer course.",
			StartDate = new DateTime(2026, 9, 1),
			EndDate = new DateTime(2026, 12, 18),
			Students =
			[
				new ApplicationUser { Id = "student-1", FirstName = "Anna", LastName = "Andersson" },
				new ApplicationUser { Id = "student-2", FirstName = "Erik", LastName = "Eriksson" },
				new ApplicationUser { Id = "student-3", FirstName = "Maria", LastName = "Svensson" }
			]
		};
	}
}
