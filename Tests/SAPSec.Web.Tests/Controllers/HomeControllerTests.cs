using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Constants;
using SAPSec.Web.Controllers;

namespace SAPSec.Web.Tests.Controllers;

public class HomeControllerTests
{
    private readonly HomeController _controller = new(null!, null!);

    [Fact]
    public void Index_Get_ReturnsViewResult()
    {
        var result = _controller.Index();

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Index_Get_SetsPageTitleInViewData()
    {
        var result = _controller.Index() as ViewResult;

        result.Should().NotBeNull();
        result.ViewData.ContainsKey(ViewDataKeys.Title).Should().BeTrue();
        result.ViewData[ViewDataKeys.Title].Should().Be(PageTitles.ServiceHome);
    }

    [Fact]
    public void Error_Get_ReturnsViewResult()
    {
        var result = _controller.Error();

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }
}