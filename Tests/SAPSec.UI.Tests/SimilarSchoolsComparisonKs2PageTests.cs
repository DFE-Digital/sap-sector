using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Web.Constants;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests.Deprecated;

[Collection("UITestsCollection")]
public class SimilarSchoolsComparisonKs2PageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string Urn = "101206";

    private async Task NavigateToKs2ComparisonPageAsync()
    {
        await Page.GotoAsync(Routes.PrimarySchool(Urn).ViewSimilarSchools);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".app-school-result a").First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(AriaRole.Link, new() { Name = "KS2", Exact = true }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task ToggleChartViewAsync()
    {
        var chartTabs = Page.Locator(".app-measure-tabs");
        await chartTabs.GetByRole(AriaRole.Button, new() { Name = "Show year by year" }).ClickAsync();
    }

    [Fact]
    public async Task Ks2Comparison_LoadsSuccessfully()
    {
        await NavigateToKs2ComparisonPageAsync();

        var heading = Page.Locator("#expected-rwm-heading");
        await Expect(heading).ToHaveTextAsync("Meeting expected standard in reading, writing and maths");
    }

    [Fact]
    public async Task Ks2Comparison_DetailsToggle_IsCollapsedByDefaultAndExpands()
    {
        await NavigateToKs2ComparisonPageAsync();

        var details = Page.Locator(".app-measure-details")
            .Filter(new() { HasText = "Information about meeting the expected standard" });

        (await details.GetAttributeAsync("open")).Should().BeNull();

        await details.Locator("summary").ClickAsync();

        (await details.GetAttributeAsync("open")).Should().NotBeNull();
    }

    [Fact]
    public async Task Ks2Comparison_ShowsExpectedBarsAndColours()
    {
        await NavigateToKs2ComparisonPageAsync();

        var barChart = Page.Locator("#expected-rwm-comparison-chart");
        var lineChart = Page.Locator("#expected-rwm-comparison-yearbyyear-chart");

        (await barChart.CountAsync()).Should().Be(1);
        (await lineChart.CountAsync()).Should().Be(1);

        var barChartData = await barChart.GetAttributeAsync("data-chart");
        barChartData.Should().NotBeNullOrWhiteSpace();
        barChartData.Should().Contain("Schools in England average");

        var barChartColours = await barChart.GetAttributeAsync("data-colors");
        barChartColours.Should().NotBeNullOrWhiteSpace();
        barChartColours.Should().Contain("#ca357c");
        barChartColours.Should().Contain("#2a1950");
    }

    [Fact]
    public async Task Ks2Comparison_ShowYearByYear_TogglesToLineChartWithStackedKey()
    {
        await NavigateToKs2ComparisonPageAsync();

        await ToggleChartViewAsync();

        var lineChart = Page.Locator("#expected-rwm-comparison-yearbyyear-chart");
        await Expect(lineChart).ToBeVisibleAsync();

        var yearByYearHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });
        await Expect(yearByYearHeading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Ks2Comparison_HasChartsAndTableTabsInOrder()
    {
        await NavigateToKs2ComparisonPageAsync();

        var tabs = Page.Locator(".app-measure-tabs .govuk-tabs__tab");
        (await tabs.AllTextContentsAsync()).Should().Equal("Charts", "Table");
    }

    [Fact]
    public async Task Ks2Comparison_SubjectFilter_HasExpectedOptionsInOrder()
    {
        await NavigateToKs2ComparisonPageAsync();

        var options = await Page.Locator("#expectedRwmSubject option").AllTextContentsAsync();
        options.Should().Equal("Reading, writing and maths", "Reading", "Writing", "Maths");
    }

    [Fact]
    public async Task Ks2Comparison_SubjectFilter_UpdatesTableCellsWithoutPageReload()
    {
        await NavigateToKs2ComparisonPageAsync();

        await Page.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var cell = Page.Locator("[data-expected-rwm-cell='this-current']");
        await Expect(cell).ToBeVisibleAsync();

        await Page.Locator("#expectedRwmSubject").SelectOptionAsync("m");
        await Page.WaitForTimeoutAsync(500);

        // The cell should still be present and rendered (percentage or "No available data") after the AJAX update.
        var text = await cell.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace();
    }
}
