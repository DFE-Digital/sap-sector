using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Controllers;

namespace SAPSec.Web.Tests.Controllers;

public class StaticContentControllerTests
{
    private readonly StaticContentController _controller = new();

    [Fact]
    public void Accessibility_Get_ReturnsViewResult()
    {
        // Act
        var result = _controller.Accessibility();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void TermsOfUse_Get_ReturnsViewResult()
    {
        // Act
        var result = _controller.TermsOfUse();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }
}