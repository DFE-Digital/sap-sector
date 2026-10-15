using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Authentication;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Web.Authorization;
using SAPSec.Web.Services;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Deprecated.Authorization;

public class SchoolAccessHandlerTests
{
    private const string Urn = "123456";
    private const string TrustUid = "5001";
    private const string LaCode = "202";

    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IRequestSchoolAccessor> _requestSchoolAccessor = new();
    private readonly Mock<IInternalStaffChecker> _internalStaffChecker = new();
    private readonly Mock<ILogger<SchoolAccessHandler>> _logger = new();
    private readonly SchoolAccessRequirement _requirement = new();
    private readonly ClaimsPrincipal _principal = new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, "user-123")],
        "TestAuth"));

    public SchoolAccessHandlerTests()
    {
        _userService.Setup(x => x.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust)]
    [InlineData(OrganisationCategoryIds.SingleAcademyTrust)]
    public async Task HandleAsync_WhenTrustUserAndSchoolInTrust_Succeeds(string categoryId)
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", Uid = TrustUid, Category = new Category { Id = categoryId } });
        SetupSchool(trustId: TrustUid, laCode: "999");

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenTrustUserAndSchoolNotInTrust_DoesNotSucceed()
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", Uid = TrustUid, Category = new Category { Id = OrganisationCategoryIds.MultiAcademyTrust } });
        SetupSchool(trustId: "9999", laCode: LaCode);

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenTrustUserAndSchoolHasNoTrust_DoesNotSucceed()
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", Uid = TrustUid, Category = new Category { Id = OrganisationCategoryIds.MultiAcademyTrust } });
        SetupSchool(trustId: null, laCode: LaCode);

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenTrustUserHasNoUid_DoesNotSucceed()
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", Category = new Category { Id = OrganisationCategoryIds.MultiAcademyTrust } });
        SetupSchool(trustId: TrustUid, laCode: LaCode);

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenLocalAuthorityUserAndSchoolInLocalAuthority_Succeeds()
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", EstablishmentNumber = LaCode, Category = new Category { Id = OrganisationCategoryIds.LocalAuthority } });
        SetupSchool(trustId: TrustUid, laCode: LaCode);

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenLocalAuthorityUserAndSchoolInOtherLocalAuthority_DoesNotSucceed()
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", EstablishmentNumber = LaCode, Category = new Category { Id = OrganisationCategoryIds.LocalAuthority } });
        SetupSchool(trustId: null, laCode: "999");

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenEstablishmentUserAndOwnUrn_SucceedsWithoutLoadingSchool()
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", Urn = Urn, Category = new Category { Id = OrganisationCategoryIds.Establishment } });

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeTrue();
        _requestSchoolAccessor.Verify(x => x.GetAsync(It.IsAny<HttpContext?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEstablishmentUserAndDifferentUrn_DoesNotSucceed()
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", Urn = "999999", Category = new Category { Id = OrganisationCategoryIds.Establishment } });

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeFalse();
        _requestSchoolAccessor.Verify(x => x.GetAsync(It.IsAny<HttpContext?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenInternalStaff_SucceedsForAnyUrnWithoutLoadingSchool()
    {
        var organisation = new Organisation { Id = "org-1", Category = new Category { Id = "011" } };
        SetupCurrentOrganisation(organisation);
        _internalStaffChecker.Setup(x => x.IsInternalStaff(organisation)).Returns(true);

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeTrue();
        _requestSchoolAccessor.Verify(x => x.GetAsync(It.IsAny<HttpContext?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDioceseUserAndSchoolInDiocese_Succeeds()
    {
        SetupCurrentOrganisation(DioceseOrganisation(uid: "CE01"));
        SetupSchool(trustId: null, laCode: LaCode, dioceseId: "CE01");

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenDioceseUserAndSchoolInOtherDiocese_DoesNotSucceed()
    {
        SetupCurrentOrganisation(DioceseOrganisation(uid: "CE01"));
        SetupSchool(trustId: null, laCode: LaCode, dioceseId: "RC02");

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeFalse();
    }

    private static Organisation DioceseOrganisation(string? uid) =>
        new()
        {
            Id = "org-1",
            Uid = uid,
            Category = new Category { Id = "TBC", Name = OrganisationCategoryIds.DioceseCategoryName }
        };

    [Fact]
    public async Task HandleAsync_WhenTrustUserAndNoUrnRouteValue_DoesNotSucceed()
    {
        SetupCurrentOrganisation(new Organisation { Id = "org-1", Uid = TrustUid, Category = new Category { Id = OrganisationCategoryIds.MultiAcademyTrust } });

        var context = await HandleAsync(CreateHttpContext(urn: null));

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentOrganisationIsNull_DoesNotSucceed()
    {
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((Organisation?)null);

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_DoesNotSucceed()
    {
        _userService.Setup(x => x.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        var context = await HandleAsync(CreateHttpContext(Urn));

        context.HasSucceeded.Should().BeFalse();
    }

    private void SetupCurrentOrganisation(Organisation organisation)
    {
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);
    }

    private void SetupSchool(string? trustId, string laCode, string? dioceseId = null)
    {
        _requestSchoolAccessor
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), Urn))
            .ReturnsAsync(CreateSchoolDetails(trustId, laCode, dioceseId));
    }

    private static HttpContext CreateHttpContext(string? urn)
    {
        var httpContext = new DefaultHttpContext();
        if (urn is not null)
        {
            httpContext.Request.RouteValues[SchoolAccessRequirement.UrnRouteValueName] = urn;
        }

        return httpContext;
    }

    private async Task<AuthorizationHandlerContext> HandleAsync(HttpContext httpContext)
    {
        var context = new AuthorizationHandlerContext([_requirement], _principal, httpContext);
        var handler = new SchoolAccessHandler(
            _userService.Object,
            _internalStaffChecker.Object,
            _requestSchoolAccessor.Object,
            _logger.Object);

        await handler.HandleAsync(context);

        return context;
    }

    private static SchoolDetails CreateSchoolDetails(string? trustId, string laCode, string? dioceseId = null) =>
        new()
        {
            Name = "Test School",
            Urn = Urn,
            ShowClosedSchoolBanner = false,
            Successors = [],
            Predecessors = [],
            DfENumber = DataWithAvailability.Available("123/4567"),
            Ukprn = DataWithAvailability.Available("10012345"),
            Address = DataWithAvailability.Available("1 Test Street"),
            LocalAuthorityName = DataWithAvailability.Available("Test LA"),
            LocalAuthorityCode = DataWithAvailability.Available(laCode),
            Region = DataWithAvailability.Available("Test Region"),
            UrbanRuralDescription = DataWithAvailability.Available("Urban"),
            AgeRangeLow = DataWithAvailability.Available(4),
            AgeRangeHigh = DataWithAvailability.Available(18),
            GenderOfEntry = DataWithAvailability.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability.Available("Primary"),
            SchoolType = DataWithAvailability.Available("Academy converter"),
            TypeOfEstablishmentCode = DataWithAvailability.Available("28"),
            AdmissionsPolicy = DataWithAvailability.Available("Not applicable"),
            ReligiousCharacter = DataWithAvailability.Available("None"),
            GovernanceStructure = DataWithAvailability.NotAvailable<GovernanceType>(),
            AcademyTrustName = DataWithAvailability.NotAvailable<string>(),
            AcademyTrustId = trustId is null
                ? DataWithAvailability.NotAvailable<string>()
                : DataWithAvailability.Available(trustId),
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(false),
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),
            HeadteacherName = DataWithAvailability.Available("Jane Smith"),
            Website = DataWithAvailability.NotAvailable<string>(),
            Telephone = DataWithAvailability.NotAvailable<string>(),
            Email = DataWithAvailability.NotAvailable<string>(),
            DioceseId = dioceseId is null
                ? DataWithAvailability.NotApplicable<string>()
                : DataWithAvailability.Available(dioceseId)
        };
}
