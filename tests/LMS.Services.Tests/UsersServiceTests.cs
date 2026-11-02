using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.Users;
using Microsoft.AspNetCore.Identity;
using Moq;
using Service.Contracts;

namespace LMS.Services.Tests
{
	/// <summary>
	/// Unit tests for creating and updating users in <see cref="UsersService"/>.
	/// </summary>
	/// <remarks>
	/// How these tests work:
	/// - UserManager and RoleManager are classes, not interfaces. Moq can still mock them,
	///   but their constructors need arguments, so a mocked store and null for the rest are passed.
	/// - ExecuteInTransactionAsync is mocked to run the action directly and to set
	///   _insideTransaction while it runs. Each Identity call checks that flag, so the tests
	///   prove that every change is made inside the transaction.
	///
	/// Not covered here:
	/// - The actual rollback in the database. That needs a real database and was tested manually:
	///   changing role and setting a duplicate email leaves the role unchanged.
	/// </remarks>
	public class UsersServiceTests
	{
		private const string UserId = "user-1";

		private readonly Mock<UserManager<ApplicationUser>> _userManager = new(
			Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

		private readonly Mock<RoleManager<IdentityRole>> _roleManager = new(
			Mock.Of<IRoleStore<IdentityRole>>(), null!, null!, null!, null!);

		private readonly Mock<IUnitOfWork> _uow = new();

		// True only while the action passed to ExecuteInTransactionAsync is running.
		private bool _insideTransaction;

		private readonly UsersService _sut;

		public UsersServiceTests()
		{
			// Run the action like the real unit of work does, and record that we are inside the transaction.
			_uow
				.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
				.Returns<Func<Task>>(async action =>
				{
					_insideTransaction = true;
					try
					{
						await action();
					}
					finally
					{
						_insideTransaction = false;
					}
				});

			// Both roles exist unless a test says otherwise.
			_roleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

			// Every Identity change succeeds, but only inside the transaction.
			_userManager
				.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.Callback(AssertInsideTransaction)
				.ReturnsAsync(IdentityResult.Success);
			_userManager
				.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.Callback(AssertInsideTransaction)
				.ReturnsAsync(IdentityResult.Success);
			_userManager
				.Setup(m => m.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
				.Callback(AssertInsideTransaction)
				.ReturnsAsync(IdentityResult.Success);
			_userManager
				.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
				.Callback(AssertInsideTransaction)
				.ReturnsAsync(IdentityResult.Success);

			_sut = new UsersService(
				_userManager.Object,
				_roleManager.Object,
				Mock.Of<ICourseService>(),
				_uow.Object);
		}

		// ---------- CreateUserAsync ----------

		/// <summary>
		/// Scenario: a teacher creates a user with a role that does not exist.
		/// Expected: 404 and no transaction is started, so nothing is created.
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_RoleDoesNotExist_ThrowsAndCreatesNothing()
		{
			_roleManager.Setup(r => r.RoleExistsAsync("Admin")).ReturnsAsync(false);

			await Assert.ThrowsAsync<EntityNotFoundException>(
				() => _sut.CreateUserAsync(CreateDto(role: "Admin")));

			_uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Never);
			_userManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
		}

		/// <summary>
		/// Scenario: a student is created without a course.
		/// Expected: 400 and nothing is created.
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_StudentWithoutCourse_ThrowsBadRequest()
		{
			await Assert.ThrowsAsync<BadRequestException>(
				() => _sut.CreateUserAsync(CreateDto(role: UserRoles.Student, courseId: null)));

			_uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Never);
		}

		/// <summary>
		/// Scenario: a valid teacher is created.
		/// Expected: the user and the role are both created inside one transaction.
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_ValidUser_CreatesUserAndRoleInOneTransaction()
		{
			await _sut.CreateUserAsync(CreateDto(role: UserRoles.Teacher));

			_uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Once);
			_userManager.Verify(m => m.CreateAsync(
				It.Is<ApplicationUser>(user => user.Email == "new@lms.com" && user.FirstName == "Ny"),
				"secret"), Times.Once);
			_userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRoles.Teacher), Times.Once);
		}

		/// <summary>
		/// Scenario: the user is created, but the role cannot be set.
		/// Expected: 400 thrown from inside the transaction, which makes the real unit of work
		/// roll back, so no user without a role is left in the database.
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_AddToRoleFails_ThrowsInsideTransaction()
		{
			_userManager
				.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.Callback(AssertInsideTransaction)
				.ReturnsAsync(Failed("Role could not be set."));

			var exception = await Assert.ThrowsAsync<BadRequestException>(
				() => _sut.CreateUserAsync(CreateDto(role: UserRoles.Teacher)));

			Assert.Equal("Role could not be set.", exception.Message);
		}

		// ---------- UpdateUserAsync ----------

		/// <summary>
		/// Scenario: the role is empty.
		/// Expected: 404 for the role, not a NullReferenceException (500).
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_EmptyRole_ThrowsEntityNotFound()
		{
			await Assert.ThrowsAsync<EntityNotFoundException>(
				() => _sut.UpdateUserAsync(UpdateDto(role: "")));
		}

		/// <summary>
		/// Scenario: the user does not exist.
		/// Expected: 404 and no transaction is started.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_UserNotFound_ThrowsEntityNotFound()
		{
			_userManager.Setup(m => m.FindByIdAsync(UserId)).ReturnsAsync((ApplicationUser?)null);

			await Assert.ThrowsAsync<EntityNotFoundException>(
				() => _sut.UpdateUserAsync(UpdateDto(role: UserRoles.Teacher)));

			_uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Never);
		}

		/// <summary>
		/// Scenario: a student is changed to teacher.
		/// Expected: the old role is removed, the new one is added and the details are saved.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_RoleChanged_ReplacesRoleAndSavesDetails()
		{
			var user = SetupExistingUser(UserRoles.Student);

			await _sut.UpdateUserAsync(UpdateDto(role: UserRoles.Teacher));

			_userManager.Verify(m => m.RemoveFromRolesAsync(user,
				It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { UserRoles.Student }))), Times.Once);
			_userManager.Verify(m => m.AddToRoleAsync(user, UserRoles.Teacher), Times.Once);
			_userManager.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(u => u.FirstName == "Uppdaterad")), Times.Once);
		}

		/// <summary>
		/// Scenario: a user has two roles and the teacher keeps one of them.
		/// Expected: only the other role is removed, so the user has exactly one role.
		/// Before this change both roles were kept, since the chosen role already existed.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_UserWithTwoRoles_KeepsOnlyChosenRole()
		{
			var user = SetupExistingUser(UserRoles.Student, UserRoles.Teacher);

			await _sut.UpdateUserAsync(UpdateDto(role: UserRoles.Teacher));

			_userManager.Verify(m => m.RemoveFromRolesAsync(user,
				It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { UserRoles.Student }))), Times.Once);
			// The user already has the chosen role, so it is not added again.
			_userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
		}

		/// <summary>
		/// Scenario: the role is changed, but saving the details fails (for example a duplicate email).
		/// Expected: 400 thrown from inside the transaction, which makes the real unit of work
		/// roll back the role change too.
		/// </summary>
		[Fact]
		public async Task UpdateUserAsync_UpdateFails_ThrowsInsideTransaction()
		{
			SetupExistingUser(UserRoles.Student);
			_userManager
				.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
				.Callback(AssertInsideTransaction)
				.ReturnsAsync(Failed("Email 'anna.andersson@lms.com' is already taken."));

			await Assert.ThrowsAsync<BadRequestException>(
				() => _sut.UpdateUserAsync(UpdateDto(role: UserRoles.Teacher)));

			// The role change happened in the same transaction, so it is rolled back with it.
			_userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRoles.Teacher), Times.Once);
		}

		// ---------- Helpers ----------

		// Fails the test if an Identity change is made outside the transaction.
		private void AssertInsideTransaction() =>
			Assert.True(_insideTransaction, "Identity changes must be made inside ExecuteInTransactionAsync.");

		private ApplicationUser SetupExistingUser(params string[] roles)
		{
			var user = new ApplicationUser { Id = UserId, Email = "old@lms.com", UserName = "old@lms.com" };
			_userManager.Setup(m => m.FindByIdAsync(UserId)).ReturnsAsync(user);
			_userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles.ToList());
			return user;
		}

		private static IdentityResult Failed(string description) =>
			IdentityResult.Failed(new IdentityError { Description = description });

		private static CreateUserDto CreateDto(string role, int? courseId = 1) => new()
		{
			Email = "new@lms.com",
			Password = "secret",
			Role = role,
			FirstName = "Ny",
			LastName = "Användare",
			CourseId = courseId
		};

		private static UpdateUserDto UpdateDto(string role) => new()
		{
			Id = UserId,
			Email = "updated@lms.com",
			Role = role,
			FirstName = "Uppdaterad",
			LastName = "Användare",
			CourseId = 1
		};
	}
}