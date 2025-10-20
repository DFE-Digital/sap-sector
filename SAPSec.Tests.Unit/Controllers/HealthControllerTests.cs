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

        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Test");
        _mockEnvironment.Setup(e => e.ApplicationName).Returns("SAPSec.Web");
        _mockEnvironment.Setup(e => e.WebRootPath).Returns("/app/wwwroot");

        _controller = new HealthController(_mockLogger.Object, _mockEnvironment.Object);
    }

    [Fact]
    public async Task Get_WhenApplicationHealthy_ReturnsOk()
    {
        // Act
        var result = await _controller.Get();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsHealthCheckResponse()
    {
        // Act
        var result = await _controller.Get();
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.IsType<HealthCheckResponse>(okResult.Value);
    }

    [Fact]
    public async Task Get_ReturnsHealthyStatus()
    {
        // Act
        var result = await _controller.Get();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as HealthCheckResponse;

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Healthy", response.Status);
    }

    [Fact]
    public async Task Get_ReturnsChecks()
    {
        // Act
        var result = await _controller.Get();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as HealthCheckResponse;

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Checks);
        Assert.Contains(response.Checks, c => c.Name == "ApplicationRunning");
        Assert.Contains(response.Checks, c => c.Name == "StaticFiles");
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
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as HealthCheckResponse;

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Healthy", response.Status);

        var appCheck = Assert.Single(response.Checks, c => c.Name == "ApplicationRunning");
        Assert.Contains(environmentName, appCheck.Message);
    }
}