using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SAPSec.Web.Constants;
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
        request.Path = Routes.SecondarySchool("123456").SchoolDetails;
        request.QueryString = new QueryString("?sort=desc");

        var result = request.GetCanonicalUrl();

        result.Should().Be($"https://service.education.gov.uk{Routes.SecondarySchool(\"123456\").SchoolDetails}");
    }

    [Fact]
    public void GetCanonicalUrl_ReturnsPrimarySchoolUrl()
    {
        var request = new DefaultHttpContext().Request;
        request.Scheme = "https";
        request.Host = new HostString("service.education.gov.uk");
        request.Path = Routes.PrimarySchool("654321").Attendance;

        var result = request.GetCanonicalUrl();

        result.Should().Be($"https://service.education.gov.uk{Routes.PrimarySchool(\"654321\").Attendance}");
    }
}
