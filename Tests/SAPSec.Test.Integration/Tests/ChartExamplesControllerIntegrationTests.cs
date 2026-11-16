using FluentAssertions;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Integration.Setup;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests;

public class ChartExamplesControllerIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string Ks2ProgressUrl = "/chart-examples/ks2-progress";

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableChartExamples);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task Ks2Progress_ReturnsNotFound_WhenFeatureFlagDisabled()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableChartExamples, false);

        await Fixture.RequestPageAsync(Ks2ProgressUrl, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ks2Progress_ShowsEveryExample()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableChartExamples, true);

        var page = await Fixture.RequestPageAsync(Ks2ProgressUrl, HttpStatusCode.OK);

        page.ElementWithTestIdShouldExist("ks2-progress-chart-examples");
        foreach (var key in new[] { "all-positive", "all-negative", "mixed", "wide-range", "extreme-negative", "extreme-positive", "extreme-both", "missing-data" })
        {
            page.ElementWithTestIdShouldExist($"{key}-example");
        }
    }

    [Theory]
    [InlineData("all-positive", "0", "0.5", "2.5")]
    [InlineData("all-negative", "-2.5", "0.5", "0")]
    [InlineData("mixed", "-2", "0.5", "1.5")]
    [InlineData("wide-range", "-25", "5", "5")]
    [InlineData("extreme-negative", "-25", "5", "5")]
    [InlineData("extreme-positive", "0", "5", "30")]
    [InlineData("extreme-both", "-30", "10", "30")]
    [InlineData("missing-data", "-0.5", "0.5", "0.5")]
    public async Task Ks2Progress_BarChartAxisIncludesZeroAndFitsTheScores(string key, string min, string step, string max)
    {
        Fixture.FeatureFlagService.Override(Flags.EnableChartExamples, true);

        var page = await Fixture.RequestPageAsync(Ks2ProgressUrl, HttpStatusCode.OK);

        var chart = page.ElementWithTestIdShouldExist($"ks2-progress-example-{key}-current-year-chart");
        chart.GetAttribute("data-axis-min").Should().Be(min);
        chart.GetAttribute("data-axis-step").Should().Be(step);
        chart.GetAttribute("data-axis-max").Should().Be(max);
    }

    [Fact]
    public async Task Ks2Progress_LineChartsKeepZeroOnTheAxis()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableChartExamples, true);

        var page = await Fixture.RequestPageAsync(Ks2ProgressUrl, HttpStatusCode.OK);

        var chart = page.ElementWithTestIdShouldExist("ks2-progress-example-mixed-year-by-year-chart");
        chart.GetAttribute("data-axis-include-zero").Should().Be("true");
    }
}
