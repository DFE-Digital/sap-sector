using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.Common.Authentication;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class AccessDeniedAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    public override async Task DisposeAsync()
    {
        Fixture.TestUser.Reset();
        await base.DisposeAsync();
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust, 1280)]
    [InlineData(OrganisationCategoryIds.SingleAcademyTrust, 1280)]
    [InlineData(OrganisationCategoryIds.LocalAuthority, 1280)]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust, 375)]
    [InlineData(OrganisationCategoryIds.SingleAcademyTrust, 375)]
    [InlineData(OrganisationCategoryIds.LocalAuthority, 375)]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust, 320)]
    [InlineData(OrganisationCategoryIds.SingleAcademyTrust, 320)]
    [InlineData(OrganisationCategoryIds.LocalAuthority, 320)]
    public async Task GroupLogin_ShowsAccessibleResponsiveAccessDeniedPage(string categoryId, int width)
    {
        Fixture.TestUser.CategoryId = categoryId;
        Fixture.TestUser.Email = TestUserOrganisation.NonDfeEmail;
        await Page.SetViewportSizeAsync(width, 900);

        var response = await Page.GotoAsync(Routes.UserHome);

        response.Should().NotBeNull();
        response!.Status.Should().Be(403);
        var main = Page.Locator("main");
        (await main.Locator("h1").InnerTextAsync()).Should().Be("You do not have access");
        (await main.Locator("h2").InnerTextAsync()).Should().Be("Access for establishment groups");
        (await main.Locator("li").AllTextContentsAsync()).Should().Equal(
            "do not have access to this school’s data",
            "are accessing the service as an establishment group, such as a trust");
        var requestAccess = main.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Request access to an organisation" });
        (await requestAccess.GetAttributeAsync("href")).Should().Be("https://services.signin.education.gov.uk/request-organisation/search");
        await requestAccess.FocusAsync();
        (await requestAccess.EvaluateAsync<bool>("element => element === document.activeElement")).Should().BeTrue();

        (await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= window.innerWidth")).Should().BeTrue(
            "the page must fit the viewport without horizontal scrolling");
        AssertNoViolations(await AnalyzePageAccessibility());
    }
}
