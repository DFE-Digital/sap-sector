using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.Authentication;
using SAPSec.Web.Authorization;
using System.Security.Claims;

namespace SAPSec.Test.Integration.Authorization;

public class BreadcrumbNavigationHandlerTests
{
    private readonly TestUserOrganisation _testUser = new() { CategoryId = OrganisationCategoryIds.Establishment };
    private bool _restrictionsEnabled = true;

    [Fact]
    public async Task SchoolUser_WithRestrictionsOn_DoesNotGetBreadcrumbs()
    {
        var context = CreateContext(authenticated: true);

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.LocalAuthority)]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust)]
    public async Task NonSchoolUser_StillGetsBreadcrumbs(string categoryId)
    {
        _testUser.CategoryId = categoryId;
        var context = CreateContext(authenticated: true);

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task SchoolUser_WithRestrictionsOff_StillGetsBreadcrumbs()
    {
        _restrictionsEnabled = false;
        var context = CreateContext(authenticated: true);

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task UserWhoIsNotSignedIn_StillGetsBreadcrumbs()
    {
        var context = CreateContext(authenticated: false);

        await CreateHandler().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    private BreadcrumbNavigationHandler CreateHandler() =>
        new(new MockDsiUserService(_testUser), new FixedFeatureFlagService(enabled: !_restrictionsEnabled));

    private static AuthorizationHandlerContext CreateContext(bool authenticated) =>
        new(
            [new BreadcrumbNavigationRequirement()],
            new ClaimsPrincipal(authenticated ? new ClaimsIdentity([new Claim("sub", "user-1")], "TestAuth") : new ClaimsIdentity()),
            null);

    private sealed class FixedFeatureFlagService(bool enabled) : IFeatureFlagService
    {
        public Task<bool> IsEnabledAsync(string featureName) => Task.FromResult(enabled);
    }
}
