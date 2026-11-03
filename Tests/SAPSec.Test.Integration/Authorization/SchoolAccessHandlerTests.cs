using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.Authentication;
using SAPSec.Web.Authorization;
using System.Security.Claims;

namespace SAPSec.Test.Integration.Authorization;

public class SchoolAccessHandlerTests
{
    private const string OwnUrn = "105574";

    private readonly TestUserOrganisation _testUser = new()
    {
        CategoryId = OrganisationCategoryIds.Establishment,
        Urn = OwnUrn
    };

    private bool _restrictionsEnabled = true;

    [Theory]
    [InlineData(OwnUrn, true)]
    [InlineData(" 105574 ", true)]
    [InlineData("108088", false)]
    [InlineData("", false)]
    public async Task SchoolUserWithUrnRoute_SucceedsOnlyForOwnSchool(string requestedUrn, bool expected)
    {
        var context = CreateContext(HttpContextWithUrn(requestedUrn));

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().Be(expected);
    }

    [Fact]
    public async Task SchoolUserWithNoUrnRoute_IsDenied()
    {
        // e.g. the Find a school search, which covers every school.
        var context = CreateContext(new DefaultHttpContext());

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task SchoolUserWithNoResource_IsDenied()
    {
        // e.g. the navigation checking the policy without a request.
        var context = CreateContext(resource: null);

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.LocalAuthority)]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust)]
    public async Task NonSchoolUser_IsNotRestrictedByThisRule(string categoryId)
    {
        _testUser.CategoryId = categoryId;
        var context = CreateContext(HttpContextWithUrn("108088"));

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("108088")]
    [InlineData("")]
    public async Task WhenRestrictionsAreSwitchedOff_SchoolUserIsNotRestricted(string requestedUrn)
    {
        _restrictionsEnabled = false;
        var context = CreateContext(HttpContextWithUrn(requestedUrn));

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task WhenUserIsNotAuthenticated_DoesNotSucceed()
    {
        var context = new AuthorizationHandlerContext(
            [new SchoolAccessRequirement()], new ClaimsPrincipal(new ClaimsIdentity()), HttpContextWithUrn(OwnUrn));

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private SchoolAccessHandler CreateHandler() =>
        new(new MockDsiUserService(_testUser), new FixedFeatureFlagService(enabled: !_restrictionsEnabled), NullLogger<SchoolAccessHandler>.Instance);

    private sealed class FixedFeatureFlagService(bool enabled) : IFeatureFlagService
    {
        public Task<bool> IsEnabledAsync(string featureName) => Task.FromResult(enabled);
    }

    private static AuthorizationHandlerContext CreateContext(object? resource) =>
        new([new SchoolAccessRequirement()], new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user-1")], "TestAuth")), resource);

    private static DefaultHttpContext HttpContextWithUrn(string urn)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues[SchoolAccessRequirement.UrnRouteValueName] = urn;
        return httpContext;
    }
}
