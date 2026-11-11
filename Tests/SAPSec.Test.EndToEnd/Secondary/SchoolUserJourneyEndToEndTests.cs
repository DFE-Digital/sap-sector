using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Core.Authentication;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Secondary;

/// <summary>
/// The journey for a user who signs in with a school organisation: straight to their own school,
/// no Find a school, and no way to reach another school by editing the URL.
/// </summary>
[Collection("EndToEndTestsCollection")]
public class SchoolUserJourneyEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string OwnSchoolUrn = "100052";
    private const string OwnSchoolName = "Hampstead School";
    private const string ComparatorSchoolUrn = "141617";
    private const string ComparatorSchoolName = "The Hurlingham Academy";
    private const string OtherSchoolUrn = ComparatorSchoolUrn;
    private const string AccessDeniedHeading = "You do not have access to Compare and connect with similar schools";

    private readonly EndToEndTestsFixture _fixture = fixture;

    public override async Task InitializeAsync()
    {
        _fixture.TestUser.CategoryId = OrganisationCategoryIds.Establishment;
        _fixture.TestUser.Urn = OwnSchoolUrn;
        _fixture.TestUser.Phase = "Secondary";

        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        _fixture.TestUser.Reset();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task SigningIn_GoesStraightToOwnSchoolOverview()
    {
        await Page.GotoAsync(Routes.UserHome);

        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(OwnSchoolUrn).Overview);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = OwnSchoolName }).First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Navigation_DoesNotOfferFindASchool()
    {
        await NavigateTo(Routes.SecondarySchool(OwnSchoolUrn).Overview);

        var navigation = Page.Locator(".govuk-service-navigation");
        await Expect(navigation.GetByText("Find a school")).ToHaveCountAsync(0);
        await Expect(navigation.GetByText("Sign out")).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("/school/secondary/100052")]
    [InlineData("/school/secondary/100052/ks4-headline-measures")]
    [InlineData("/school/secondary/100052/view-similar-schools")]
    [InlineData("/school/secondary/100052/what-is-a-similar-school")]
    [InlineData("/cookies")]
    [InlineData("/terms-and-conditions")]
    public async Task Pages_DoNotShowBreadcrumbs(string path)
    {
        await NavigateTo(path);

        await Expect(Page.Locator(".govuk-breadcrumbs")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ComparisonPage_ShowsBackButton_NotBreadcrumbs_AndBackReturnsToSimilarSchools()
    {
        await NavigateTo(Routes.SecondarySchool(OwnSchoolUrn).Comparison(ComparatorSchoolUrn).Similarity);

        await Expect(Page.Locator(".govuk-breadcrumbs")).ToHaveCountAsync(0);
        var back = Page.Locator("a.govuk-back-link");
        await Expect(back).ToBeVisibleAsync();
        await Expect(back).ToHaveTextAsync("View similar schools");

        await back.ClickAsync();

        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(OwnSchoolUrn).ViewSimilarSchools);
    }

    [Fact]
    public async Task ServiceName_ReturnsToOwnSchoolOverview()
    {
        await NavigateTo(Routes.SecondarySchool(OwnSchoolUrn).SchoolDetails);

        await Page.Locator(".govuk-service-navigation__service-name").GetByRole(AriaRole.Link).ClickAsync();

        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(OwnSchoolUrn).Overview);
    }

    [Theory]
    [InlineData("/find-a-school")]
    [InlineData("/find-a-school?query=Hampstead")]
    public async Task FindASchool_ShowsAccessDenied(string path)
    {
        var response = await Page.GotoAsync(path);

        response!.Status.Should().Be(403);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = AccessDeniedHeading })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ChangingTheUrnToAnotherSchool_ShowsAccessDenied()
    {
        var response = await Page.GotoAsync(Routes.SecondarySchool(OtherSchoolUrn).Overview);

        response!.Status.Should().Be(403);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = AccessDeniedHeading })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AnotherSchoolsSimilarSchoolsList_ShowsAccessDenied()
    {
        var response = await Page.GotoAsync(Routes.SecondarySchool(ComparatorSchoolUrn).ViewSimilarSchools);

        response!.Status.Should().Be(403);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = AccessDeniedHeading })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task OwnSimilarSchools_CanBeOpenedAndCompared()
    {
        await NavigateTo(Routes.SecondarySchool(OwnSchoolUrn).Overview);

        await Page.GetByText("View similar schools", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(OwnSchoolUrn).ViewSimilarSchools);

        await Page.GetByText(ComparatorSchoolName, new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(OwnSchoolUrn).Comparison(ComparatorSchoolUrn).Similarity);
    }

    [Fact]
    public async Task ComparingToAnotherSchool_DoesNotLeadToThatSchoolsSimilarSchools()
    {
        await NavigateTo(Routes.SecondarySchool(OwnSchoolUrn).Comparison(ComparatorSchoolUrn).Similarity);

        var response = await Page.GotoAsync(Routes.SecondarySchool(ComparatorSchoolUrn).ViewSimilarSchools);

        response!.Status.Should().Be(403);
    }
}
