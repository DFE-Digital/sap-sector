using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class ComparisonSchoolDetailsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string CurrentSchoolUrn = "100001";
    private const string SimilarSchoolUrn = "100002";

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(CurrentSchoolUrn, "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment(SimilarSchoolUrn, "Test School 2", x => x.Open().Primary().InLA("002")));

        return base.InitializeAsync();
    }

    [Fact]
    public async Task HeadingAndTitle_ReflectCurrentAndComparatorSchools()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(CurrentSchoolUrn).Comparison(SimilarSchoolUrn).SchoolDetails);

        page.Title.Should().Be("Test School 2 - Get school improvement insights - GOV.UK");

        var heading = page.ElementShouldExist("h1.govuk-heading-xl");
        heading.TrimmedTextContent().Should().Be("Test School 2");

        var caption = page.ElementShouldExist(".govuk-caption-xl");
        caption.TrimmedTextContent().Should().Be("Test School 1");
    }
}
