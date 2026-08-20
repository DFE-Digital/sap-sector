using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core;
using SAPSec.Web.Middleware;

namespace SAPSec.Web.Tests.Middleware;

public class NotFoundExceptionHandlerTests
{
    private readonly Mock<ILogger<NotFoundExceptionHandler>> _loggerMock = new();
    private readonly Mock<IHostEnvironment> _hostEnvironmentMock = new();

    [Fact]
    public async Task TryHandleAsync_WhenNotFoundException_ReturnsTrue_Sets404_AndLogsWarning()
    {
        var sut = CreateSut(isProduction: false);
        var context = new DefaultHttpContext();
        context.Request.Path = "/school/999999/ks4-headline-measures";
        var exception = new NotFoundException("School not found with URN: 999999");

        var handled = await sut.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Request.Path.ToString().Should().Be("/error/404");
        context.Items["ErrorMessage"].Should().Be(exception.Message);
        VerifyLog(LogLevel.Warning, exception.Message, Times.Once());
    }

    [Fact]
    public async Task TryHandleAsync_WhenUnhandledException_ReturnsFalse_AndLogsError()
    {
        var sut = CreateSut(isProduction: false);
        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("Boom");

        var handled = await sut.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        context.Items["ErrorMessage"].Should().Be(exception.Message);
        VerifyLog(LogLevel.Error, exception.Message, Times.Once());
    }

    private NotFoundExceptionHandler CreateSut(bool isProduction)
    {
        _hostEnvironmentMock.Setup(x => x.EnvironmentName)
            .Returns(isProduction ? Environments.Production : Environments.Development);

        return new NotFoundExceptionHandler(_loggerMock.Object, _hostEnvironmentMock.Object);
    }

    private void VerifyLog(LogLevel level, string message, Times times)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == message),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
