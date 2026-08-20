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
        var schoolDetailsUrl = Routes.SecondarySchool("123456").SchoolDetails;
        var request = new DefaultHttpContext().Request;
        request.Scheme = "https";
        request.Host = new HostString("service.education.gov.uk");
        request.Path = schoolDetailsUrl;
        request.QueryString = new QueryString("?sort=desc");

        var result = request.GetCanonicalUrl();

        result.Should().Be($"https://service.education.gov.uk{schoolDetailsUrl}");
    }

    [Fact]
    public void GetCanonicalUrl_ReturnsPrimarySchoolUrl()
    {
        var attendanceUrl = Routes.PrimarySchool("654321").Attendance;
        var request = new DefaultHttpContext().Request;
        request.Scheme = "https";
        request.Host = new HostString("service.education.gov.uk");
        request.Path = attendanceUrl;

        var result = request.GetCanonicalUrl();

        result.Should().Be($"https://service.education.gov.uk{attendanceUrl}");
    }
}
