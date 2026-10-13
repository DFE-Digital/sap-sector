using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.Services;

namespace SAPSec.Web.Tests.Deprecated.Services;

public class RequestSchoolAccessorTests
{
    private readonly Mock<ILinkedSchoolService> _schoolServiceMock = new();

    [Fact]
    public async Task GetAsync_WhenHttpContextIsNull_LoadsFromServiceEachTime()
    {
        var school = CreateSchool("123456");
        _schoolServiceMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(school);

        var sut = new RequestSchoolAccessor(_schoolServiceMock.Object);

        var first = await sut.GetAsync(null, "123456");
        var second = await sut.GetAsync(null, "123456");

        first.Should().BeSameAs(school);
        second.Should().BeSameAs(school);
        _schoolServiceMock.Verify(x => x.GetByUrnAsync("123456"), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAsync_WhenHttpContextContainsCachedSchool_ReturnsCachedValue()
    {
        var httpContext = new DefaultHttpContext();
        var school = CreateSchool("123456");
        _schoolServiceMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(school);

        var sut = new RequestSchoolAccessor(_schoolServiceMock.Object);

        var first = await sut.GetAsync(httpContext, "123456");
        var second = await sut.GetAsync(httpContext, "123456");

        first.Should().BeSameAs(school);
        second.Should().BeSameAs(school);
        _schoolServiceMock.Verify(x => x.GetByUrnAsync("123456"), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenDifferentUrnsAreRequested_CachesPerUrn()
    {
        var httpContext = new DefaultHttpContext();
        var firstSchool = CreateSchool("123456");
        var secondSchool = CreateSchool("654321");

        _schoolServiceMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(firstSchool);
        _schoolServiceMock
            .Setup(x => x.GetByUrnAsync("654321"))
            .ReturnsAsync(secondSchool);

        var sut = new RequestSchoolAccessor(_schoolServiceMock.Object);

        var first = await sut.GetAsync(httpContext, "123456");
        var second = await sut.GetAsync(httpContext, "654321");
        var firstAgain = await sut.GetAsync(httpContext, "123456");

        first.Should().BeSameAs(firstSchool);
        second.Should().BeSameAs(secondSchool);
        firstAgain.Should().BeSameAs(firstSchool);
        _schoolServiceMock.Verify(x => x.GetByUrnAsync("123456"), Times.Once);
        _schoolServiceMock.Verify(x => x.GetByUrnAsync("654321"), Times.Once);
    }

    private static LinkedSchool CreateSchool(string urn) => new(
        urn,
        "Test School",
        new("123", "Test LA"),
        new("1 Test Street"),
        EstablishmentStatusId.OpenId,
        PhaseOfEducationId.SecondaryId,
        null,
        false,
        [],
        []);
}
