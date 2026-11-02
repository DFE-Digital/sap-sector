using FluentAssertions;
using Microsoft.Extensions.Options;
using SAPSec.Core.Authentication;
using SAPSec.Test.Common.Authentication;
using SAPSec.Web.Authorization;
using System.Security.Claims;

namespace SAPSec.Test.Integration.Authorization;

public class DfeUserIdentifierTests
{
    private static readonly string[] DfeOrganisationNames = ["Department for Education"];

    [Theory]
    [InlineData("Department for Education")]
    [InlineData("DEPARTMENT FOR EDUCATION")]
    [InlineData(" Department for Education ")]
    public async Task SignedInAsDfeOrganisation_IsDfeUser(string organisationName)
    {
        (await IsDfeUserAsync(organisationName, DfeOrganisationNames)).Should().BeTrue();
    }

    [Theory]
    [InlineData("Wetherby High School")]
    [InlineData("Department for Education Academy")]
    [InlineData("Department for Work and Pensions")]
    [InlineData("")]
    public async Task SignedInAsAnotherOrganisation_IsNotDfeUser(string organisationName)
    {
        (await IsDfeUserAsync(organisationName, DfeOrganisationNames)).Should().BeFalse();
    }

    [Fact]
    public async Task DfeEmailSignedInAsSchool_IsNotDfeUser()
    {
        var identifier = CreateIdentifier("Wetherby High School", DfeOrganisationNames, OrganisationCategoryIds.Establishment);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "user-1"), new Claim("email", "jane.smith@education.gov.uk")],
            "TestAuth"));

        (await identifier.IsDfeUserAsync(principal)).Should().BeFalse();
    }

    [Fact]
    public async Task NoConfiguredOrganisationNames_NobodyIsDfeUser()
    {
        (await IsDfeUserAsync("Department for Education", [])).Should().BeFalse();
    }

    [Fact]
    public async Task UserWhoIsNotSignedIn_IsNotDfeUser()
    {
        var identifier = CreateIdentifier("Department for Education", DfeOrganisationNames);

        (await identifier.IsDfeUserAsync(new ClaimsPrincipal(new ClaimsIdentity()))).Should().BeFalse();
    }

    private static Task<bool> IsDfeUserAsync(string organisationName, string[] configuredNames) =>
        CreateIdentifier(organisationName, configuredNames)
            .IsDfeUserAsync(new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user-1")], "TestAuth")));

    private static DfeUserIdentifier CreateIdentifier(
        string organisationName,
        string[] configuredNames,
        string categoryId = "011") =>
        new(
            new MockDsiUserService(new TestUserOrganisation { Name = organisationName, CategoryId = categoryId }),
            Options.Create(new DfeUserOptions { OrganisationNames = configuredNames }));
}
