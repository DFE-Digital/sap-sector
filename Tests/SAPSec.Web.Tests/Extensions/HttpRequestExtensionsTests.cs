using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SAPSec.Web.Extensions;

namespace SAPSec.Web.Tests.Extensions;

public class HttpRequestExtensionsTests
{
    [Fact]
    public void GetCanonicalUrl_ReturnsSecondarySchoolUrlWithoutQueryString()
    {
        var request = new DefaultHttpContext().Request;
        request.Scheme = "https";
        request.Host = new HostString("service.education.gov.uk");
        request.Path = "/school/123456/school-details";
        request.QueryString = new QueryString("?sort=desc");

        var result = request.GetCanonicalUrl();

        result.Should().Be("https://service.education.gov.uk/school/123456/school-details");
    }

    [Fact]
    public void GetCanonicalUrl_ReturnsPrimarySchoolUrl()
    {
        var request = new DefaultHttpContext().Request;
        request.Scheme = "https";
        request.Host = new HostString("service.education.gov.uk");
        request.Path = "/school/primary/654321/attendance";

        var result = request.GetCanonicalUrl();

        result.Should().Be("https://service.education.gov.uk/school/primary/654321/attendance");
    }
}
