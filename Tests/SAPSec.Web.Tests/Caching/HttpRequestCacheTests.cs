using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SAPSec.Core.Caching;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolDetails.School;
using SAPSec.Core.UseCases;
using SAPSec.Web.Caching;

namespace SAPSec.Web.Tests.Caching;

public class HttpRequestCacheTests
{
    private readonly Mock<IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse>> _schoolDetailsUseCaseMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();

    [Fact]
    public async Task GetAsync_WhenHttpContextIsNull_LoadsFromServiceEachTime()
    {
        var school = CreateSchoolDetails("123456");
        _schoolDetailsUseCaseMock
            .Setup(x => x.Execute(It.Is<GetSchoolDetailsRequest>(r => r.Urn == "123456")))
            .ReturnsAsync(new GetSchoolDetailsResponse(school));
        
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        var cache = new HttpRequestCache(_httpContextAccessorMock.Object);

        var sut = new CachedUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse>(_schoolDetailsUseCaseMock.Object, cache);

        var first = await sut.Execute(new GetSchoolDetailsRequest("123456"));
        var second = await sut.Execute(new GetSchoolDetailsRequest("123456"));

        first.SchoolDetails.Should().BeSameAs(school);
        second.SchoolDetails.Should().BeSameAs(school);
        _schoolDetailsUseCaseMock.Verify(x => x.Execute(new GetSchoolDetailsRequest("123456")), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAsync_WhenHttpContextContainsCachedSchool_ReturnsCachedValue()
    {
        var httpContext = new DefaultHttpContext();
        var school = CreateSchoolDetails("123456");
        _schoolDetailsUseCaseMock
            .Setup(x => x.Execute(It.Is<GetSchoolDetailsRequest>(r => r.Urn == "123456")))
            .ReturnsAsync(new GetSchoolDetailsResponse(school));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var cache = new HttpRequestCache(_httpContextAccessorMock.Object);
        var sut = new CachedUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse>(_schoolDetailsUseCaseMock.Object, cache);

        var first = await sut.Execute(new GetSchoolDetailsRequest("123456"));
        var second = await sut.Execute(new GetSchoolDetailsRequest("123456"));

        first.SchoolDetails.Should().BeSameAs(school);
        second.SchoolDetails.Should().BeSameAs(school);
        _schoolDetailsUseCaseMock.Verify(x => x.Execute(It.Is<GetSchoolDetailsRequest>(r => r.Urn == "123456")), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenDifferentUrnsAreRequested_CachesPerUrn()
    {
        var httpContext = new DefaultHttpContext();
        var firstSchool = CreateSchoolDetails("123456");
        var secondSchool = CreateSchoolDetails("654321");

        _schoolDetailsUseCaseMock
            .Setup(x => x.Execute(It.Is<GetSchoolDetailsRequest>(r => r.Urn == "123456")))
            .ReturnsAsync(new GetSchoolDetailsResponse(firstSchool));
        _schoolDetailsUseCaseMock
            .Setup(x => x.Execute(It.Is<GetSchoolDetailsRequest>(r => r.Urn == "654321")))
            .ReturnsAsync(new GetSchoolDetailsResponse(secondSchool));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var cache = new HttpRequestCache(_httpContextAccessorMock.Object);
        var sut = new CachedUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse>(_schoolDetailsUseCaseMock.Object, cache);

        var first = await sut.Execute(new GetSchoolDetailsRequest("123456"));
        var second = await sut.Execute(new GetSchoolDetailsRequest("654321"));
        var firstAgain = await sut.Execute(new GetSchoolDetailsRequest("123456"));

        first.SchoolDetails.Should().BeSameAs(firstSchool);
        second.SchoolDetails.Should().BeSameAs(secondSchool);
        firstAgain.SchoolDetails.Should().BeSameAs(firstSchool);
        _schoolDetailsUseCaseMock.Verify(x => x.Execute(It.Is<GetSchoolDetailsRequest>(r => r.Urn == "123456")), Times.Once);
        _schoolDetailsUseCaseMock.Verify(x => x.Execute(It.Is<GetSchoolDetailsRequest>(r => r.Urn == "654321")), Times.Once);
    }

    private static SchoolDetails CreateSchoolDetails(string urn) =>
        new()
        {
            Name = "Test School",
            Urn = urn,
            ShowClosedSchoolBanner = false,
            Successors = [],
            Predecessors = [],
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
            TypeOfEstablishmentCode = DataWithAvailability.Available("1"),
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
