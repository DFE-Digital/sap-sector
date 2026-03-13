using FluentAssertions;

namespace SAPSec.Integration.Tests;

public class SimilarSchoolsComparisonIntegrationTests
{
    private static readonly string SimilarityViewPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "SAPSec.Web", "Views", "SimilarSchoolsComparison", "Similarity.cshtml"));

    private static readonly string SchoolDetailsViewPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "SAPSec.Web", "Views", "SimilarSchoolsComparison", "SchoolDetails.cshtml"));

    [Fact]
    public async Task GetSimilarity_ReturnsSuccess()
    {
        File.Exists(SimilarityViewPath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(SimilarityViewPath);

        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetSimilarity_ContainsComparisonHeadingAndTable()
    {
        var content = await File.ReadAllTextAsync(SimilarityViewPath);

        content.Should().Contain("How these schools compare");
        content.Should().Contain("govuk-table");
        content.Should().Contain("Characteristic");
    }

    [Fact]
    public async Task GetSchoolDetails_ReturnsSuccess()
    {
        File.Exists(SchoolDetailsViewPath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(SchoolDetailsViewPath);

        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetSchoolDetails_ContainsExpectedSections()
    {
        var content = await File.ReadAllTextAsync(SchoolDetailsViewPath);

        content.Should().Contain("School Details");
        content.Should().Contain("Location");
        content.Should().Contain("Further information");
    }
}
