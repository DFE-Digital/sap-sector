using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewComponents;
using SAPSec.Web.ViewModels.Components;

namespace SAPSec.Web.Tests.ViewComponents;

public class CookiesViewComponentTests
{
    [Fact]
    public void Invoke_WithNoCookiePreference_ReturnsUnselectedBanner()
    {
        var sut = CreateSut();

        var result = sut.Invoke();

        var viewResult = result.Should().BeOfType<ViewViewComponentResult>().Subject;
        var model = viewResult.ViewData!.Model.Should().BeOfType<CookiesViewModel>().Subject;
        model.BannerState.Should().Be("unselected");
    }

    [Fact]
    public void Invoke_WithCookiePreferenceAndNoBannerQuery_ReturnsEmptyContent()
    {
        var sut = CreateSut(ctx =>
        {
            ctx.Request.Headers.Cookie = $"{LayoutConstants.CookieSettingsName}=enabled";
        });

        var result = sut.Invoke();

        result.Should().BeOfType<EmptyContentView>();
    }

    [Theory]
    [InlineData("accepted")]
    [InlineData("rejected")]
    public void Invoke_WithCookiePreferenceAndBannerQuery_ReturnsBannerState(string bannerState)
    {
        var sut = CreateSut(ctx =>
        {
            ctx.Request.Headers.Cookie = $"{LayoutConstants.CookieSettingsName}=enabled";
            ctx.Request.QueryString = new QueryString($"?cookie-banner={bannerState}");
        });

        var result = sut.Invoke();

        var viewResult = result.Should().BeOfType<ViewViewComponentResult>().Subject;
        var model = viewResult.ViewData!.Model.Should().BeOfType<CookiesViewModel>().Subject;
        model.BannerState.Should().Be(bannerState);
    }

    private static CookiesViewComponent CreateSut(Action<DefaultHttpContext>? configureContext = null)
    {
        var httpContext = new DefaultHttpContext();
        configureContext?.Invoke(httpContext);

        var sut = new CookiesViewComponent
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new()
                {
                    HttpContext = httpContext,
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                }
            }
        };

        return sut;
    }
}
