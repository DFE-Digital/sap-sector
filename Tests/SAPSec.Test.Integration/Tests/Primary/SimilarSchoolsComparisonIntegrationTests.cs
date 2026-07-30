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

    [Fact]
    public async Task SimilarSchoolComparison_Ks2_DisplaysMeetingExpectedStandardSection()
    {
        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment(PrimarySchoolUrn, x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment(SimilarSchoolUrn, x => x.WithRwmExpected(current: "60", prev: "61", prev2: "62")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "61", prev: "60", prev2: "59")));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparisonKs2(SimilarSchoolUrn));

        var heading = page.QuerySelector("#expected-rwm-heading");
        heading.Should().NotBeNull();
        heading!.TrimmedTextContent().Should().Be("Meeting expected standard in reading, writing and maths");

        var details = page.QuerySelectorAll("details.govuk-details")
            .FirstOrDefault(d => d.QuerySelector(".govuk-details__summary-text")?.TrimmedTextContent()
                == "Information about meeting the expected standard");
        details.Should().NotBeNull();
        details!.HasAttribute("open").Should().BeFalse();

        var subjectSelect = page.QuerySelector("#expectedRwmSubject");
        subjectSelect.Should().NotBeNull();
        var options = subjectSelect!.QuerySelectorAll("option").Select(o => o.TrimmedTextContent()).ToList();
        options.Should().Equal("Reading, writing and maths", "Reading", "Writing", "Maths");

        var tabs = page.QuerySelector(".app-measure-tabs");
        tabs.Should().NotBeNull();
        var tabLabels = tabs!.QuerySelectorAll(".govuk-tabs__tab").Select(t => t.TrimmedTextContent()).ToList();
        tabLabels.Should().Equal("Charts", "Table");

        var tableRows = page.QuerySelector("#expected-rwm-table-view table")!
            .QuerySelectorAll("tbody tr").Select(r => r.QuerySelectorAll("th, td").Select(c => c.TrimmedTextContent()).ToArray());

        tableRows.Should().BeEquivalentTo(new[]
        {
            new[] { "Test School 1", "79%", "80%", "81%" },
            new[] { "Test School 2", "62%", "61%", "60%" },
            new[] { "Schools in England average", "59%", "60%", "61%" }
        });
    }
}
