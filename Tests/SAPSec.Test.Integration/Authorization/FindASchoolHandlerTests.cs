using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.Authentication;
using SAPSec.Web.Authorization;
using System.Security.Claims;

namespace SAPSec.Test.Integration.Authorization;

public class FindASchoolHandlerTests
{
    private readonly TestUserOrganisation _testUser = new();
    private bool _restrictionsEnabled = true;

    [Theory]
    [InlineData(OrganisationCategoryIds.LocalAuthority)]
    [InlineData("011")]
    public async Task DfeUser_WithRestrictionsOn_CanUseFindASchool(string categoryId)
    {
        _testUser.CategoryId = categoryId;
        _testUser.Name = TestUserOrganisation.DfeOrganisationName;

        var context = await HandleAsync(authenticated: true);

        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.Establishment)]
    [InlineData(OrganisationCategoryIds.LocalAuthority)]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust)]
    [InlineData(OrganisationCategoryIds.SingleAcademyTrust)]
    public async Task NonDfeUser_WithRestrictionsOn_CannotUseFindASchool(string categoryId)
    {
        _testUser.CategoryId = categoryId;
        _testUser.Name = TestUserOrganisation.NonDfeOrganisationName;

        var context = await HandleAsync(authenticated: true);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task UserWithDfeEmail_SignedInAsTheirSchool_CannotUseFindASchool()
    {
        // DfE staff are recognised by the organisation they sign in as, not their email address.
        _testUser.CategoryId = OrganisationCategoryIds.Establishment;
        _testUser.Name = "Wetherby High School";

        var context = await HandleAsync(authenticated: true, email: "jane.smith@education.gov.uk");

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task NonDfeUser_WithRestrictionsOff_CanUseFindASchool()
    {
        _restrictionsEnabled = false;
        _testUser.Name = TestUserOrganisation.NonDfeOrganisationName;

        var context = await HandleAsync(authenticated: true);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task UserWhoIsNotSignedIn_CannotUseFindASchool()
    {
        _restrictionsEnabled = false;

        var context = await HandleAsync(authenticated: false);

        context.HasSucceeded.Should().BeFalse();
    }

    private async Task<AuthorizationHandlerContext> HandleAsync(bool authenticated, string email = "test@example.com")
    {
        var context = new AuthorizationHandlerContext(
            [new FindASchoolRequirement()],
            new ClaimsPrincipal(authenticated
                ? new ClaimsIdentity([new Claim("sub", "user-1"), new Claim("email", email)], "TestAuth")
                : new ClaimsIdentity()),
            null);

        var userService = new MockDsiUserService(_testUser);
        var handler = new FindASchoolHandler(
            userService,
            new FixedFeatureFlagService(enabled: !_restrictionsEnabled),
            new DfeUserIdentifier(userService, Options.Create(new DfeUserOptions { OrganisationNames = [TestUserOrganisation.DfeOrganisationName] })));

        await handler.HandleAsync(context);

        return context;
    }

    private sealed class FixedFeatureFlagService(bool enabled) : IFeatureFlagService
    {
        public Task<bool> IsEnabledAsync(string featureName) => Task.FromResult(enabled);
    }
}
