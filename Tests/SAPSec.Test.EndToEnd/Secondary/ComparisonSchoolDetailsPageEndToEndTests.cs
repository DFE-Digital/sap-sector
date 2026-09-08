using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Secondary;

[Collection("EndToEndTestsCollection")]
public class ComparisonSchoolDetailsPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string CurrentSchoolUrn = "100052";
    private const string CurrentSchoolName = "Hampstead School";
    private const string ComparatorSchoolUrn = "141617";
    private const string ComparatorSchoolName = "The Hurlingham Academy";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(CurrentSchoolUrn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Overview);
        await Page.GetByText("View similar schools", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).ViewSimilarSchools);
        await Page.GetByText(ComparatorSchoolName, new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Comparison(ComparatorSchoolUrn).Similarity);
        await Page.GetByText("School details", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Comparison(ComparatorSchoolUrn).SchoolDetails);
    }

    [Fact]
    public async Task SchoolDetails_IdentifiesCorrectSchool()
    {
        await Expect(Page.GetByDefinitionTerm("ID")).ToContainTextAsync($"URN: {ComparatorSchoolUrn}");
        await Expect(Page.Locator(".govuk-caption-xl")).ToHaveTextAsync(CurrentSchoolName);
        await Expect(Page.Locator(".govuk-heading-xl")).ToHaveTextAsync(ComparatorSchoolName);
    }

    [Fact]
    public async Task SchoolDetails_DisplaysMapDetails_Component()
    {
        var details = Page.Locator("details#comparison-map-details.govuk-details");
        var count = await details.CountAsync();

        count.Should().Be(1, "Map details component should be present");

        var summaryText = details.Locator("summary .govuk-details__summary-text");
        (await summaryText.TextContentAsync()).Should().Contain("View on a map");
    }

    [Fact]
    public async Task SchoolDetails_MapContainer_HasExpectedAttributes()
    {
        await Page.WaitForSelectorAsync("#map", new() { State = WaitForSelectorState.Attached, Timeout = 15000 });

        var map = Page.Locator("#map");
        (await map.CountAsync()).Should().Be(1, "Map container should exist");

        var mapMode = await map.GetAttributeAsync("data-map-mode");
        mapMode.Should().Be("compare");

        (await map.GetAttributeAsync("data-fixed-zoom")).Should().Be("14");
        (await map.GetAttributeAsync("role")).Should().Be("region");
        (await map.GetAttributeAsync("aria-label")).Should().Be("Map of schools");

        var loading = map.Locator(".map-loading");
        (await loading.CountAsync()).Should().BeGreaterThanOrEqualTo(0);

        if (await loading.CountAsync() > 0)
        {
            (await loading.First.TextContentAsync()).Should().Contain("Loading map");
        }
    }

    [Fact]
    public async Task SchoolDetails_MapLegend_HasBothMarkerIcons()
    {
        var mainMarker = Page.Locator("img.school-marker-icon[src='/assets/images/marker-school-pink.svg']");
        var similarMarker = Page.Locator("img.school-marker-icon[src='/assets/images/marker-school.svg']");

        (await mainMarker.CountAsync()).Should().Be(1, "Main school marker icon (pink) should be present");
        (await similarMarker.CountAsync()).Should().Be(1, "Similar school marker icon (blue) should be present");
    }
}
