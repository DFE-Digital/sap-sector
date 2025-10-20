using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Web.Controllers;
using Xunit;

namespace SAPSec.Tests.Unit.Controllers;

public class HealthControllerTests
{
    private readonly Mock<ILogger<HealthController>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _mockLogger = new Mock<ILogger<HealthController>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();

        // Create a real temp directory
        var tempPath = Path.Combine(Path.GetTempPath(), "SAPSecTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);

        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Test");
        _mockEnvironment.Setup(e => e.ApplicationName).Returns("SAPSec.Web");
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(tempPath);

        _controller = new HealthController(_mockLogger.Object, _mockEnvironment.Object);
    }

    [Fact]
    public async Task Get_WhenApplicationHealthy_ReturnsOk()
    {
        // Act
        var result = await _controller.Get();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsHealthCheckResponse()
    {
        // Act
        var result = await _controller.Get();

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // The type is in SAPSec.Web.Controllers namespace
        Assert.IsType<HealthCheckResponse>(okResult.Value);
    }

    [Fact]
    public async Task Get_ReturnsHealthyStatus()
    {
        // Act
        var result = await _controller.Get();

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var response = Assert.IsType<HealthCheckResponse>(okResult.Value);
        Assert.Equal("Healthy", response.Status);
        Assert.NotEmpty(response.Checks);
    }

    [Fact]
    public async Task Get_ReturnsAllChecks()
    {
        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(okResult.Value);

        // Should have 2 checks: ApplicationRunning and StaticFiles
        Assert.Equal(2, response.Checks.Count);
        Assert.Contains(response.Checks, c => c.Name == "ApplicationRunning");
        Assert.Contains(response.Checks, c => c.Name == "StaticFiles");
    }

    [Fact]
    public async Task Get_ApplicationRunningCheck_PassesInTestEnvironment()
    {
        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(okResult.Value);

        var appCheck = response.Checks.FirstOrDefault(c => c.Name == "ApplicationRunning");
        Assert.NotNull(appCheck);
        Assert.Equal("Pass", appCheck.Status);
        Assert.Contains("Test environment", appCheck.Message);
        Assert.Contains("SAPSec.Web", appCheck.Message);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Get_WorksInAllEnvironments(string environmentName)
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(environmentName);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(okResult.Value);

        Assert.Equal("Healthy", response.Status);

        var appCheck = response.Checks.FirstOrDefault(c => c.Name == "ApplicationRunning");
        Assert.NotNull(appCheck);
        Assert.Contains(environmentName, appCheck.Message);
    }

    [Fact]
    public async Task Get_WhenNoWwwroot_ReturnsUnhealthy()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.WebRootPath).Returns((string)null!);

        // Act
        var result = await _controller.Get();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);

        var response = Assert.IsType<HealthCheckResponse>(statusCodeResult.Value);
        Assert.Equal("Unhealthy", response.Status);

        var staticFilesCheck = response.Checks.FirstOrDefault(c => c.Name == "StaticFiles");
        Assert.NotNull(staticFilesCheck);
        Assert.Equal("Fail", staticFilesCheck.Status);
    }

    [Fact]
    public async Task Get_ReturnsTimestamp()
    {
        // Arrange
        var beforeCall = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = await _controller.Get();
        var afterCall = DateTime.UtcNow.AddSeconds(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(okResult.Value);

        Assert.True(response.Timestamp > beforeCall);
        Assert.True(response.Timestamp < afterCall);
    }
}