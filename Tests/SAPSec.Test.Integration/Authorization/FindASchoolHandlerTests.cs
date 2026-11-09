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
        _testUser.Email = TestUserOrganisation.DfeEmail;

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
        _testUser.Email = TestUserOrganisation.NonDfeEmail;

        var context = await HandleAsync(authenticated: true);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task UserWithDfeEmail_SignedInAsASchool_CanUseFindASchool()
    {
        // DfE staff are recognised by their email address, whatever organisation they sign in as.
        _testUser.CategoryId = OrganisationCategoryIds.Establishment;
        _testUser.Name = "Wetherby High School";
        _testUser.Email = TestUserOrganisation.DfeEmail;

        var context = await HandleAsync(authenticated: true);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task NonDfeUser_WithRestrictionsOff_CanUseFindASchool()
    {
        _restrictionsEnabled = false;
        _testUser.Email = TestUserOrganisation.NonDfeEmail;

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

    private async Task<AuthorizationHandlerContext> HandleAsync(bool authenticated)
    {
        var context = new AuthorizationHandlerContext(
            [new FindASchoolRequirement()],
            new ClaimsPrincipal(authenticated
                ? new ClaimsIdentity([new Claim("sub", "user-1")], "TestAuth")
                : new ClaimsIdentity()),
            null);

        var userService = new MockDsiUserService(_testUser);
        var handler = new FindASchoolHandler(
            userService,
            new FixedFeatureFlagService(enabled: !_restrictionsEnabled),
            new DfeUserIdentifier(userService, Options.Create(new DfeUserOptions { EmailDomains = [TestUserOrganisation.DfeEmailDomain] })));

        await handler.HandleAsync(context);

        return context;
    }

    private sealed class FixedFeatureFlagService(bool enabled) : IFeatureFlagService
    {
        public Task<bool> IsEnabledAsync(string featureName) => Task.FromResult(enabled);
    }
}
