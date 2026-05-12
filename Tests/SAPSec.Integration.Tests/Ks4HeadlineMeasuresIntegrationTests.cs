using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;
using System.Text.RegularExpressions;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class Ks4HeadlineMeasuresIntegrationTests(WebApplicationSetupFixture fixture)
{
    private const string Ks4HeadlineMeasuresPath = "/school/105574/ks4-headline-measures";

    [Fact]
    public async Task Ks4HeadlineMeasures_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(Ks4HeadlineMeasuresPath);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_ContainsExpectedSections()
    {
        var response = await fixture.Client.GetAsync(Ks4HeadlineMeasuresPath);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("KS4 headline performance measures");
        content.Should().Contain("Progress 8");
        content.Should().Contain("Attainment 8");
        content.Should().Contain("3-year average");
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_DestinationsDetails_UsesNumericThreeYearAverageWording()
    {
        var response = await fixture.Client.GetAsync(Ks4HeadlineMeasuresPath);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Information about staying in education or entering employment data");
        content.Should().Contain("The most recent data covers pupils who finished year 11 in 2023. The 3-year average is the average of pupils completed year 11 in 2021,2022 and 2023.");
        content.Should().NotContain("The most recent data covers pupils who finished year 11 in 2023. The three-year average is the average of pupils completed year 11 in 2021,2022 and 2023.");
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_TopPerformersContainLinksToSimilarSchoolComparison()
    {
        var response = await fixture.Client.GetAsync(Ks4HeadlineMeasuresPath);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("/school/105574/view-similar-schools/");
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_BarChartsUseExpectedDecimalPlaces()
    {
        var response = await fixture.Client.GetAsync(Ks4HeadlineMeasuresPath);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        GetCanvasMarkup(content, "ks4-attainment8-school-chart").Should().Contain("data-label-decimals=\"1\"");
        GetCanvasMarkup(content, "eng-maths-school-chart").Should().Contain("data-label-decimals=\"0\"");
        GetCanvasMarkup(content, "destinations-school-chart").Should().Contain("data-label-decimals=\"0\"");
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_Attainment8YearByYear_DisablesAxisAutoSkip()
    {
        var response = await fixture.Client.GetAsync(Ks4HeadlineMeasuresPath);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        GetCanvasMarkup(content, "ks4-attainment8-school-yearbyyear-chart").Should().Contain("data-axis-auto-skip=\"false\"");
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_WithNonExistentUrn_ReturnsNotFound()
    {
        var response = await fixture.Client.GetAsync("/school/999999/ks4-headline-measures");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    private static string GetCanvasMarkup(string content, string id)
    {
        var pattern = $"""<canvas[^>]*id="{Regex.Escape(id)}"[^>]*>""";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline);

        match.Success.Should().BeTrue($"expected canvas '{id}' to be rendered");
        return match.Value;
    }
}
