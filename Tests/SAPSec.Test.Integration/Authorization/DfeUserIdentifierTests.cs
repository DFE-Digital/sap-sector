using FluentAssertions;
using Microsoft.Extensions.Options;
using SAPSec.Core.Authentication;
using SAPSec.Test.Common.Authentication;
using SAPSec.Web.Authorization;
using System.Security.Claims;

namespace SAPSec.Test.Integration.Authorization;

public class DfeUserIdentifierTests
{
    private static readonly string[] DfeEmailDomains = ["education.gov.uk"];

    [Theory]
    [InlineData("jane.smith@education.gov.uk")]
    [InlineData("Jane.Smith@EDUCATION.GOV.UK")]
    [InlineData(" jane.smith@education.gov.uk ")]
    public async Task UserWithDfeEmail_IsDfeUser(string email)
    {
        (await IsDfeUserAsync(email, DfeEmailDomains)).Should().BeTrue();
    }

    [Theory]
    [InlineData("jane.smith@school.org.uk")]
    [InlineData("jane.smith@digital.education.gov.uk")]
    [InlineData("jane.smith@education.gov.uk.example.com")]
    [InlineData("jane.smith@noteducation.gov.uk")]
    [InlineData("education.gov.uk")]
    [InlineData("jane.smith@")]
    [InlineData("")]
    public async Task UserWithAnotherEmail_IsNotDfeUser(string email)
    {
        (await IsDfeUserAsync(email, DfeEmailDomains)).Should().BeFalse();
    }

    [Fact]
    public async Task UserWithDfeEmail_SignedInAsASchool_IsStillDfeUser()
    {
        (await IsDfeUserAsync("jane.smith@education.gov.uk", DfeEmailDomains, OrganisationCategoryIds.Establishment))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ConfiguredDomainWithLeadingAt_StillMatches()
    {
        (await IsDfeUserAsync("jane.smith@education.gov.uk", ["@education.gov.uk"])).Should().BeTrue();
    }

    [Fact]
    public async Task NoConfiguredEmailDomains_NobodyIsDfeUser()
    {
        (await IsDfeUserAsync("jane.smith@education.gov.uk", [])).Should().BeFalse();
    }

    [Fact]
    public async Task UserWhoIsNotSignedIn_IsNotDfeUser()
    {
        var identifier = CreateIdentifier("jane.smith@education.gov.uk", DfeEmailDomains);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        (await identifier.IsDfeUserAsync(principal)).Should().BeFalse();
    }

    private static Task<bool> IsDfeUserAsync(
        string email,
        string[] configuredDomains,
        string categoryId = OrganisationCategoryIds.LocalAuthority) =>
        CreateIdentifier(email, configuredDomains, categoryId)
            .IsDfeUserAsync(new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user-1")], "TestAuth")));

    private static DfeUserIdentifier CreateIdentifier(
        string email,
        string[] configuredDomains,
        string categoryId = OrganisationCategoryIds.LocalAuthority) =>
        new(
            new MockDsiUserService(new TestUserOrganisation { CategoryId = categoryId, Email = email }),
            Options.Create(new DfeUserOptions { EmailDomains = configuredDomains }));
}
