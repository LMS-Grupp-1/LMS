using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using LMS.Presentation.Controllers;
using LMS.Shared.DTOs.ModulesDtos;
using Service.Contracts;

namespace LMS.Presentation.Tests;
public class ModulesControllerTests
{
    private readonly Mock<IServiceManager> _serviceManagerMock;
    private readonly Mock<IModuleService> _moduleServiceMock;
    private readonly ModulesController _sut; // System Under Test

    public ModulesControllerTests()
    {
        _serviceManagerMock = new Mock<IServiceManager>();
        _moduleServiceMock = new Mock<IModuleService>();

        // Wire up IServiceManager to return our mock IModuleService
        _serviceManagerMock.Setup(s => s.ModuleService).Returns(_moduleServiceMock.Object);

        // Inject the mocked ServiceManager into the controller
        _sut = new ModulesController(_serviceManagerMock.Object);
    }

    [Fact]
    public async Task GetModules_WhenCourseExists_ReturnsOkWithModules()
    {
        // Arrange
        int courseId = 1;
        var expectedModules = new List<ModuleDto>
        {
            new ModuleDto { Id = 10, Name = "C# Basics", CourseId = courseId }
        };

        _moduleServiceMock.Setup(m => m.GetModulesByCourseIdAsync(courseId))
            .ReturnsAsync(expectedModules);

        // Act
        var result = await _sut.GetModules(courseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedModules = Assert.IsAssignableFrom<IEnumerable<ModuleDto>>(okResult.Value);

        Assert.Single(returnedModules);
    }

    [Fact]
    public async Task CreateModule_WhenValidDto_ReturnsOkTrue()
    {
        // Arrange
        int courseId = 1;
        var dto = new CreateModuleDto
        {
            Name = "New Module",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        _moduleServiceMock.Setup(m => m.CreateModuleAsync(courseId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateModule(courseId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool?)okResult.Value);

        // Verify service method was actually called
        _moduleServiceMock.Verify(m => m.CreateModuleAsync(courseId, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateModule_WhenValidDto_ReturnsOkTrue()
    {
        // Arrange
        int courseId = 1;
        int moduleId = 10;
        var dto = new CreateModuleDto
        {
            Name = "Updated Module",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        _moduleServiceMock.Setup(m => m.UpdateModuleAsync(courseId, moduleId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateModule(courseId, moduleId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool?)okResult.Value);

        _moduleServiceMock.Verify(m => m.UpdateModuleAsync(courseId, moduleId, dto), Times.Once);
    }
}