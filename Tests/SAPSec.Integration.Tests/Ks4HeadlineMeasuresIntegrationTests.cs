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
    public async Task Ks4HeadlineMeasures_WithNonExistentUrn_ReturnsNotFound()
    {
        var response = await fixture.Client.GetAsync("/school/999999/ks4-headline-measures");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
