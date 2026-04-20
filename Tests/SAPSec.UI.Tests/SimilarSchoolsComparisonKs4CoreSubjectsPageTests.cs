using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SimilarSchoolsComparisonKs4CoreSubjectsPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string Path = "/school/108088/view-similar-schools/137621/Ks4CoreSubjects";

    [Fact]
    public async Task Ks4CoreSubjectsComparison_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(Path);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task Ks4CoreSubjectsComparison_ShowsExpectedBarsAndColours()
    {
        await Page.GotoAsync(Path);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var barCharts = Page.Locator("canvas[id$='-comparison-chart'][data-type='bar']");
        await Expect(barCharts).ToHaveCountAsync(7);

        var englishLanguageChart = Page.Locator("#english-language-comparison-chart");
        var chartData = await englishLanguageChart.GetAttributeAsync("data-chart");
        chartData.Should().NotBeNullOrWhiteSpace();
        chartData.Should().Contain("Schools in England average");

        var chartColours = await englishLanguageChart.GetAttributeAsync("data-colors");
        chartColours.Should().NotBeNullOrWhiteSpace();
        chartColours.Should().Contain("#ca357c");
        chartColours.Should().Contain("#2a1950");
    }

    [Fact]
    public async Task Ks4CoreSubjectsComparison_YearByYearChartsUsePercentAxisInQuarters()
    {
        await Page.GotoAsync(Path);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var lineCharts = Page.Locator("canvas[id$='-comparison-yearbyyear-chart'][data-type='line']");
        await Expect(lineCharts).ToHaveCountAsync(7);

        for (var index = 0; index < await lineCharts.CountAsync(); index++)
        {
            var chart = lineCharts.Nth(index);
            (await chart.GetAttributeAsync("data-axis-min")).Should().Be("0");
            (await chart.GetAttributeAsync("data-axis-step")).Should().Be("25");
            (await chart.GetAttributeAsync("data-axis-max")).Should().Be("100");
            (await chart.GetAttributeAsync("data-axis-auto-skip")).Should().Be("false");
            (await chart.GetAttributeAsync("data-axis-suffix")).Should().Be("%");
        }
    }
}
