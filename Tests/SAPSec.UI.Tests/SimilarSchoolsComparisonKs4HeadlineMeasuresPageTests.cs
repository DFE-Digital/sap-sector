using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SimilarSchoolsComparisonKs4HeadlineMeasuresPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string Path = "/school/108088/view-similar-schools/137621/Ks4HeadlineMeasures";

    [Fact]
    public async Task Ks4HeadlineMeasuresComparison_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(Path);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task Ks4HeadlineMeasuresComparison_ShowsExpectedBarsAndColours()
    {
        await Page.GotoAsync(Path);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var barChart = Page.Locator("#ks4-attainment8-comparison-chart");
        var lineChart = Page.Locator("#ks4-attainment8-comparison-yearbyyear-chart");
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
}
