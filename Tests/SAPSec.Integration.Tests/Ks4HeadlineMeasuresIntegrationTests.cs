using FluentAssertions;

namespace SAPSec.Integration.Tests;

public class Ks4HeadlineMeasuresIntegrationTests
{
    private static readonly string ViewPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "SAPSec.Web", "Views", "School", "Ks4HeadlineMeasures.cshtml"));

    [Fact]
    public async Task Ks4HeadlineMeasures_ViewExists()
    {
        File.Exists(ViewPath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(ViewPath);
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Ks4HeadlineMeasures_ViewContainsExpectedSections()
    {
        var content = await File.ReadAllTextAsync(ViewPath);

        content.Should().Contain("KS4 headline performance measures");
        content.Should().Contain("Progress 8");
        content.Should().Contain("Attainment 8");
        content.Should().Contain("3 year average");
    }
}
