using FluentAssertions;
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
        var result = await _controller.Get();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Get_ReturnsHealthCheckResponse()
    {
        var result = await _controller.Get();
        var okResult = result as OkObjectResult;

        okResult!.Value.Should().BeOfType<HealthCheckResponse>();
    }
}