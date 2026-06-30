using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Services;

namespace SAPSec.Web.Tests.Services;

public class RequestSchoolAccessorTests
{
    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock = new();

    [Fact]
    public async Task GetAsync_WhenHttpContextIsNull_LoadsFromServiceEachTime()
    {
        var school = CreateSchoolDetails("123456");
        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(school);

        var sut = new RequestSchoolAccessor(_schoolDetailsServiceMock.Object);

        var first = await sut.GetAsync(null, "123456");
        var second = await sut.GetAsync(null, "123456");

        first.Should().BeSameAs(school);
        second.Should().BeSameAs(school);
        _schoolDetailsServiceMock.Verify(x => x.GetByUrnAsync("123456"), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAsync_WhenHttpContextContainsCachedSchool_ReturnsCachedValue()
    {
        var httpContext = new DefaultHttpContext();
        var school = CreateSchoolDetails("123456");
        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(school);

        var sut = new RequestSchoolAccessor(_schoolDetailsServiceMock.Object);

        var first = await sut.GetAsync(httpContext, "123456");
        var second = await sut.GetAsync(httpContext, "123456");

        first.Should().BeSameAs(school);
        second.Should().BeSameAs(school);
        _schoolDetailsServiceMock.Verify(x => x.GetByUrnAsync("123456"), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenDifferentUrnsAreRequested_CachesPerUrn()
    {
        var httpContext = new DefaultHttpContext();
        var firstSchool = CreateSchoolDetails("123456");
        var secondSchool = CreateSchoolDetails("654321");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("123456"))
            .ReturnsAsync(firstSchool);
        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("654321"))
            .ReturnsAsync(secondSchool);

        var sut = new RequestSchoolAccessor(_schoolDetailsServiceMock.Object);

        var first = await sut.GetAsync(httpContext, "123456");
        var second = await sut.GetAsync(httpContext, "654321");
        var firstAgain = await sut.GetAsync(httpContext, "123456");

        first.Should().BeSameAs(firstSchool);
        second.Should().BeSameAs(secondSchool);
        firstAgain.Should().BeSameAs(firstSchool);
        _schoolDetailsServiceMock.Verify(x => x.GetByUrnAsync("123456"), Times.Once);
        _schoolDetailsServiceMock.Verify(x => x.GetByUrnAsync("654321"), Times.Once);
    }

    private static SchoolDetails CreateSchoolDetails(string urn) =>
        new()
        {
            Name = "Test School",
            Urn = urn,
            DfENumber = DataWithAvailability.Available("123/4567"),
            Ukprn = DataWithAvailability.Available("10012345"),
            Address = DataWithAvailability.Available("1 Test Street"),
            LocalAuthorityName = DataWithAvailability.Available("Test LA"),
            LocalAuthorityCode = DataWithAvailability.Available("123"),
            Region = DataWithAvailability.Available("Test Region"),
            UrbanRuralDescription = DataWithAvailability.Available("Urban"),
            AgeRangeLow = DataWithAvailability.Available(11),
            AgeRangeHigh = DataWithAvailability.Available(18),
            GenderOfEntry = DataWithAvailability.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability.Available("Secondary"),
            SchoolType = DataWithAvailability.Available("Academy converter"),
            AdmissionsPolicy = DataWithAvailability.Available("Non-selective"),
            ReligiousCharacter = DataWithAvailability.Available("None"),
            GovernanceStructure = DataWithAvailability.Available(GovernanceType.MultiAcademyTrust),
            AcademyTrustName = DataWithAvailability.Available("Test Trust"),
            AcademyTrustId = DataWithAvailability.Available("5001"),
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(true),
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),
            HeadteacherName = DataWithAvailability.Available("Jane Smith"),
            Website = DataWithAvailability.NotAvailable<string>(),
            Telephone = DataWithAvailability.NotAvailable<string>(),
            Email = DataWithAvailability.NotAvailable<string>()
        };
}
