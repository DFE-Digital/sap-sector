using FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;

namespace SAPSec.Test.Integration.Tests;

[Collection("JsonRepositoryIntegrationTestsCollection")]
public class SimilarSchoolsComparisonIntegrationTests(JsonRepositoryIntegrationTestFixture fixture)
{
    private static readonly string ComparisonOverviewPath =
        Routes.SecondarySchool("108088").Comparison("137621").Similarity;
    private static readonly string ComparisonSchoolDetailsPath =
        Routes.SecondarySchool("108088").Comparison("137621").SchoolDetails;

    [Fact]
    public async Task GetSimilarity_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(ComparisonOverviewPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSimilarity_ContainsComparisonHeadingAndTable()
    {
        var response = await fixture.Client.GetAsync(ComparisonOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("How these schools compare");
        content.Should().Contain("govuk-table");
        content.Should().Contain("Characteristic");
    }

    [Fact]
    public async Task GetSchoolDetails_HomeBreadcrumb_LinksToSchoolSearch()
    {
        var response = await fixture.Client.GetAsync(ComparisonSchoolDetailsPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("href=\"/find-a-school\">Home</a>");
    }

    [Fact]
    public async Task GetSchoolDetails_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(ComparisonSchoolDetailsPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSchoolDetails_ContainsExpectedSections()
    {
        var response = await fixture.Client.GetAsync(ComparisonSchoolDetailsPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("School Details");
        content.Should().Contain("Location");
        content.Should().Contain("Further information");
    }
}
