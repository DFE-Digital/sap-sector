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
}
