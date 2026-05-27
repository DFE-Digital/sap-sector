using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using SAPSec.Core.Configuration;
using SAPSec.Core.Features.Home;
using SAPSec.Core.Features.Home.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class HomeControllerTests
{
    private readonly HomeController _controller;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<IFeatureFlagService> _mockFeatureFlagService;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly string _signInUri = "https://example.com/signin";

    public HomeControllerTests()
    {
        _mockEnvironment = new();
        _mockFeatureFlagService = new();
        _mockUrlHelper = new();
        Mock<IOptions<DfeSignInSettings>> options = new();
        var getEnablePrimarySchools = new GetEnablePrimarySchools(_mockFeatureFlagService.Object);

        _controller = new(options.Object, _mockEnvironment.Object, getEnablePrimarySchools)
        {
            Url = _mockUrlHelper.Object
        };

        options.Setup(x => x.Value).Returns(new DfeSignInSettings { SignInUri = _signInUri });
        _mockFeatureFlagService
            .Setup(x => x.IsEnabledAsync(FeatureFlags.EnablePrimarySchools))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task Index_Get_In_Production_Environment_ReturnsViewResult()
    {
        _mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Production");
        var result = await _controller.Index();

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var homeViewModel = (result as ViewResult)!.Model as HomeViewModel;
        homeViewModel.Should().NotBeNull();
        homeViewModel.StartNowUri.Should().Be(_signInUri);
        homeViewModel.EnablePrimarySchools.Should().BeFalse();
    }

    [Fact]
    public async Task Index_Get_In_Development_Environment_ReturnsViewResult()
    {
        _mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");
        _mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/school-search");

        var result = await _controller.Index();

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var homeViewModel = (result as ViewResult)!.Model as HomeViewModel;
        homeViewModel.Should().NotBeNull();
        homeViewModel.StartNowUri.Should().Be("/school-search");
        homeViewModel.EnablePrimarySchools.Should().BeFalse();
    }
}
