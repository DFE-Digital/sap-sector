using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

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
        content.Should().Contain("id=\"ks4-attainment8-school-chart\"");
        content.Should().Contain("data-label-decimals=\"0\"");
        content.Should().Contain("id=\"eng-maths-school-chart\" class=\"js-chart js-chart--school-ks4-bar\" data-type=\"bar\" data-show-no-data-labels=\"true\" data-axis-step=\"25\" data-axis-max=\"100\" data-axis-suffix=\"%\" data-label-decimals=\"0\"");
        content.Should().Contain("id=\"destinations-school-chart\" class=\"js-chart js-chart--school-ks4-bar\" data-type=\"bar\" data-show-no-data-labels=\"true\" data-axis-step=\"25\" data-axis-max=\"100\" data-axis-suffix=\"%\" data-label-decimals=\"0\"");
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_WithNonExistentUrn_ReturnsNotFound()
    {
        var response = await fixture.Client.GetAsync("/school/999999/ks4-headline-measures");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
