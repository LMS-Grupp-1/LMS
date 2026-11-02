
using Moq;
using Xunit;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Services;
using LMS.Shared.DTOs.ModulesDtos;


namespace LMS.Services.Tests;

public class ModuleServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ModuleService _sut; // System Under Test

    public ModuleServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ModuleService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetModulesByCourseIdAsync_WhenCourseDoesNotExist_ThrowsEntityNotFoundException()
    {
        // Arrange
        int courseId = 1;

        _unitOfWorkMock.Setup(u => u.Courses.GetByIdAsync(courseId, false))
            .ReturnsAsync((Course?)null);

        // Act & Assert (Standard xUnit)
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.GetModulesByCourseIdAsync(courseId)
        );

        Assert.Contains("Course", exception.Message);
    }

    [Fact]
    public async Task CreateModuleAsync_WhenModuleOverlaps_ThrowsBadRequestException()
    {
        // Arrange
        int courseId = 1;
        var course = new Course
        {
            Id = courseId,
            StartDate = new DateTime(2026, 01, 01),
            EndDate = new DateTime(2026, 12, 31)
        };

        var existingModules = new List<Module>
        {
            new Module
            {
                Id = 1,
                Name = "Existing Module",
                StartDate = new DateTime(2026, 05, 01),
                EndDate = new DateTime(2026, 05, 10),
                CourseId = courseId
            }
        };

        var newDto = new CreateModuleDto
        {
            Name = "New Module",
            StartDate = new DateTime(2026, 05, 05),
            EndDate = new DateTime(2026, 05, 15)
        }; // Overlaps

        _unitOfWorkMock.Setup(u => u.Courses.GetByIdAsync(courseId, false)).ReturnsAsync(course);
        _unitOfWorkMock.Setup(u => u.Modules.GetModulesByCourseIdAsync(courseId)).ReturnsAsync(existingModules);

        await Assert.ThrowsAsync<BadRequestException>(
            () => _sut.CreateModuleAsync(courseId, newDto)
        );
    }

    [Fact]
    public async Task UpdateModuleAsync_WhenModuleDoesNotExist_ThrowsEntityNotFoundException()
    {
        // Arrange
        int courseId = 1;
        int moduleId = 99;
        var dto = new CreateModuleDto
        {
            Name = "Updated Module",
            StartDate = new DateTime(2026, 06, 01),
            EndDate = new DateTime(2026, 06, 10)
        };

        _unitOfWorkMock.Setup(u => u.Modules.GetByIdAsync(moduleId))
            .ReturnsAsync((Module?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.UpdateModuleAsync(courseId, moduleId, dto)
        );
    }

    [Fact]
    public async Task UpdateModuleAsync_WhenModuleBelongsToDifferentCourse_ThrowsEntityNotFoundException()
    {
        // Arrange
        int courseId = 1;
        int moduleId = 10;
        var existingModule = new Module
        {
            Id = moduleId,
            CourseId = 99
        }; // Different course ID than the route parameter!

        var dto = new CreateModuleDto
        {
            Name = "Updated Module",
            StartDate = new DateTime(2026, 06, 01),
            EndDate = new DateTime(2026, 06, 10)
        };

        _unitOfWorkMock.Setup(u => u.Modules.GetByIdAsync(moduleId))
            .ReturnsAsync(existingModule);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.UpdateModuleAsync(courseId, moduleId, dto)
        );
    }

    [Fact]
    public async Task UpdateModuleAsync_WhenValidInput_UpdatesModuleAndCompletes()
    {
        // Arrange
        int courseId = 1;
        int moduleId = 10;

        var course = new Course
        {
            Id = courseId,
            Name = "Test Course",
            StartDate = new DateTime(2026, 01, 01),
            EndDate = new DateTime(2026, 12, 31)
        };

        var existingModule = new Module
        {
            Id = moduleId,
            CourseId = courseId,
            Name = "Old Name",
            StartDate = new DateTime(2026, 05, 01),
            EndDate = new DateTime(2026, 05, 10)
        };

        var dto = new CreateModuleDto
        {
            Name = "Updated Module Name",
            StartDate = new DateTime(2026, 06, 01),
            EndDate = new DateTime(2026, 06, 10)
        };

        // Setup mocks for validation and retrieval
        _unitOfWorkMock.Setup(u => u.Modules.GetByIdAsync(moduleId)).ReturnsAsync(existingModule);
        _unitOfWorkMock.Setup(u => u.Courses.GetByIdAsync(courseId, false)).ReturnsAsync(course);
        _unitOfWorkMock.Setup(u => u.Modules.GetModulesByCourseIdAsync(courseId)).ReturnsAsync(new List<Module> { existingModule });

        _unitOfWorkMock
            .Setup(u => u.Activities.GetActivityByModuleIdAsync(moduleId))
            .ReturnsAsync(Array.Empty<Activity>());

        // Act
        await _sut.UpdateModuleAsync(courseId, moduleId, dto);

        // Assert: Verify properties were updated on the entity object
        Assert.Equal("Updated Module Name", existingModule.Name);

        // Assert: Verify that repository update and unit of work completion were triggered
        _unitOfWorkMock.Verify(u => u.Modules.Update(existingModule), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}