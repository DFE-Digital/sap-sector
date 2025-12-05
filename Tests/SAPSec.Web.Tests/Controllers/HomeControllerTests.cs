using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using SAPSec.Core.Configuration;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class HomeControllerTests
{
    private readonly HomeController _controller;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly string _signInUri = "https://example.com/signin";

    public HomeControllerTests()
    {
        _mockEnvironment = new();
        _mockUrlHelper = new();
        Mock<IOptions<DfeSignInSettings>> options = new();

        _controller = new(options.Object, _mockEnvironment.Object)
        {
            Url = _mockUrlHelper.Object
        };

        options.Setup(x => x.Value).Returns(new DfeSignInSettings { SignInUri = _signInUri });
    }

    [Fact]
    public void Index_Get_In_Production_Environment_ReturnsViewResult()
    {
        _mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Production");
        var result = _controller.Index();

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var homeViewModel = (result as ViewResult)!.Model as HomeViewModel;
        homeViewModel.Should().NotBeNull();
        homeViewModel.StartNowUri.Should().Be(_signInUri);
    }

    [Fact]
    public void Index_Get_In_Development_Environment_ReturnsViewResult()
    {
        _mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");
        _mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/school-search");

        var result = _controller.Index();

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var homeViewModel = (result as ViewResult)!.Model as HomeViewModel;
        homeViewModel.Should().NotBeNull();
        homeViewModel.StartNowUri.Should().Be("/school-search");
    }
}