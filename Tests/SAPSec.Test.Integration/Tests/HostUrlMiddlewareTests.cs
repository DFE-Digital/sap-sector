namespace SAPSec.Test.Integration.Tests;

public class HostUrlMiddlewareTests
{
    //[Fact]
    //public async Task Show_HostUrlBanner_When_Not_CCSS_Url()
    //{
    //    var middleware = new HostUrlBannerMiddleware(next: (context) => Task.CompletedTask);

    //    var context = new DefaultHttpContext();

    //    context.Request.Host = new HostString("get-school-improvement-insights.education.gov.uk");

    //    await middleware.InvokeAsync(context);

    //    Assert.False((bool)context.Items["ShowHostUrlBanner"]);
    //}

    //[Fact]
    //public async Task Hide_HostUrlBanner_When_Url_Is_CCSS_Url()
    //{
    //    var middleware = new HostUrlBannerMiddleware(next: (context) => Task.CompletedTask);

    //    var context = new DefaultHttpContext();

    //    context.Request.Host = new HostString("compare-connect-similar-schools.education.gov.uk");

    //    await middleware.InvokeAsync(context);

    //    Assert.True((bool)context.Items["ShowHostUrlBanner"]);
    //}
}