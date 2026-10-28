using Microsoft.AspNetCore.Http;
using SAPSec.Web.Middleware;

namespace SAPSec.Test.Integration.Tests;

public class HostUrlMiddlewareTests
{
    [Theory]
    [InlineData("get-school-improvement-insights.education.gov.uk")]
    [InlineData("get-school-improvement-insights-test.test.teacherservices.cloud")]
    public async Task Show_HostUrlBanner_When_Not_CCSS_Url(string host)
    {
        var middleware = new HostUrlBannerMiddleware(next: (context) => Task.CompletedTask);

        var context = new DefaultHttpContext();

        context.Request.Host = new HostString(host);

        await middleware.InvokeAsync(context);

        Assert.True((bool)context.Items["ShowHostUrlBanner"]);
    }

    [Theory]
    [InlineData("compare-connect-similar-schools.education.gov.uk")]
    [InlineData("compare-connect-similar-schools-test.test.teacherservices.cloud")]
    [InlineData("some-other-url.education.gov.uk")]
    public async Task Hide_HostUrlBanner_When_Url_Is_CCSS_Url(string host)
    {
        var middleware = new HostUrlBannerMiddleware(next: (context) => Task.CompletedTask);

        var context = new DefaultHttpContext();

        context.Request.Host = new HostString(host);

        await middleware.InvokeAsync(context);

        Assert.False((bool)context.Items["ShowHostUrlBanner"]);
    }
}