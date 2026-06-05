using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SchoolKs4HeadlineMeasuresPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string Path = "/school/105574/ks4-headline-measures";

    private async Task NavigateAsync()
    {
        await Page.GotoAsync(Path, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "KS4 headline performance measures" })).ToBeVisibleAsync();
        await Expect(Page.Locator("#ks4-attainment8-school-chart")).ToBeVisibleAsync();
    }

    private async Task ToggleChartViewAsync(int chartGroupIndex = 0)
    {
        var chartTabs = Page.Locator(".app-ks4-tabs").Nth(chartGroupIndex);
        await chartTabs.GetByRole(AriaRole.Button, new() { Name = "Show year by year" }).ClickAsync();
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(Path, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "KS4 headline performance measures" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_UsesExpectedColoursForAttainmentCharts()
    {
        await NavigateAsync();

        var barChart = Page.Locator("#ks4-attainment8-school-chart");
        var lineChart = Page.Locator("#ks4-attainment8-school-yearbyyear-chart");

        await Expect(barChart).ToBeVisibleAsync();
        await ToggleChartViewAsync();
        await Expect(lineChart).ToBeVisibleAsync();

        var barColours = await barChart.EvaluateAsync<string[]>(@"
            el => {
                const chart = window.Chart && window.Chart.getChart(el);
                return chart?.data?.datasets?.[0]?.backgroundColor ?? [];
            }
        ");

        var lineColours = await lineChart.EvaluateAsync<string[]>(@"
            el => {
                const chart = window.Chart && window.Chart.getChart(el);
                return chart?.data?.datasets?.map(dataset => dataset.borderColor) ?? [];
            }
        ");

        barColours.Should().Equal("#ca357c", "#2a1950", "#2a1950", "#2a1950");
        lineColours.Should().Equal("#ca357c", "#2a1950", "#5694ca", "#4b9b7d");
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_Attainment8YearByYear_ShowsExpectedYAxisTicks()
    {
        await NavigateAsync();

        await ToggleChartViewAsync();
        var lineChart = Page.Locator("#ks4-attainment8-school-yearbyyear-chart");
        await Expect(lineChart).ToBeVisibleAsync();

        var axis = await lineChart.EvaluateAsync<JsonElement>(@"
            el => {
                const chart = window.Chart && window.Chart.getChart(el);
                return {
                    min: chart?.scales?.y?.min ?? null,
                    max: chart?.scales?.y?.max ?? null,
                    ticks: chart?.scales?.y?.ticks?.map(tick => tick.label) ?? []
                };
            }
        ");

        var min = axis.GetProperty("min").GetDouble();
        var max = axis.GetProperty("max").GetDouble();
        var ticks = axis.GetProperty("ticks").EnumerateArray().Select(tick => tick.GetString()).ToArray();

        min.Should().Be(0d);
        max.Should().BeGreaterThan(min);
        (max - min).Should().BeLessThan(90d);
        ticks.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_UsesExpectedColoursForAllSchoolCharts()
    {
        await NavigateAsync();

        var barChartSelectors = new[]
        {
            "#ks4-attainment8-school-chart",
            "#eng-maths-school-chart",
            "#destinations-school-chart"
        };

        foreach (var selector in barChartSelectors)
        {
            var chart = Page.Locator(selector);
            var colours = await chart.EvaluateAsync<string[]>(@"
                el => {
                    const chart = window.Chart && window.Chart.getChart(el);
                    return chart?.data?.datasets?.[0]?.backgroundColor ?? [];
                }
            ");

            colours.Should().Equal("#ca357c", "#2a1950", "#2a1950", "#2a1950");
        }

        await ToggleChartViewAsync(0);
        await ToggleChartViewAsync(1);
        await ToggleChartViewAsync(2);

        var lineChartSelectors = new[]
        {
            "#ks4-attainment8-school-yearbyyear-chart",
            "#eng-maths-school-yearbyyear-chart",
            "#destinations-school-yearbyyear-chart"
        };

        foreach (var selector in lineChartSelectors)
        {
            var chart = Page.Locator(selector);
            var colours = await chart.EvaluateAsync<string[]>(@"
                el => {
                    const chart = window.Chart && window.Chart.getChart(el);
                    return chart?.data?.datasets?.map(dataset => dataset.borderColor) ?? [];
                }
            ");

            colours.Should().Equal("#ca357c", "#2a1950", "#5694ca", "#4b9b7d");
        }
    }
}
