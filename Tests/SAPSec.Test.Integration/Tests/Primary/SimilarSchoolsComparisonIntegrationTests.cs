using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class SimilarSchoolsComparisonIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string PrimarySchoolUrn = "100001";
    private const string SimilarSchoolUrn = "100002";

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(PrimarySchoolUrn, "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment(SimilarSchoolUrn, "Test School 2", x => x.Open().Primary().InLA("002")));

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnablePrimarySchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task SimilarSchoolComparison_SchoolDetails_HeadingAndTitle_ReflectComparisonPage()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonSchoolDetails(SimilarSchoolUrn));

        page.Title.Should().Be("School details compared to Test School 2 - Get school improvement insights - GOV.UK");

        var heading = page.QuerySelector("h1.govuk-heading-xl");
        heading.Should().NotBeNull();
        heading.TrimmedTextContent().Should().Be("Test School 2");

        var caption = page.QuerySelector(".govuk-caption-xl");
        caption.Should().NotBeNull();
        caption.TrimmedTextContent().Should().Be("Test School 1");
    }

    [Fact]
    public async Task SimilarSchoolComparison_Ks2_DisplaysProgressScoreSectionWithDetailsToggle()
    {
        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

        var tabHeading = page.QuerySelector("h2.govuk-heading-l");
        tabHeading.Should().NotBeNull();
        tabHeading.TrimmedTextContent().Should().Be("KS2 performance measures");

        var progressHeading = page.QuerySelector("#progress-rwm-heading");
        progressHeading.Should().NotBeNull();
        progressHeading.TrimmedTextContent().Should().Be("Progress score in reading, writing and maths");

        var details = page.QuerySelector("details.govuk-details");
        details.Should().NotBeNull();

        var summary = details!.QuerySelector(".govuk-details__summary-text");
        summary.Should().NotBeNull();
        summary.TrimmedTextContent().Should().Be("Information about progress score in reading, writing and maths");

        var insetPanel = page.QuerySelector(".app-measure-message-panel");
        insetPanel.Should().NotBeNull();
        insetPanel.TrimmedTextContent().Should().Contain("There are no KS1-KS2 progress scores");
    }
}
