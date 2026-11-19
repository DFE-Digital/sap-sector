using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Controllers;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class Ks2ProgressNumberOfPupilsExampleIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string Path = ComponentExamplesController.Ks2ProgressNumberOfPupilsPath;

    private const string Hint =
        "The number of eligible pupils included in this measure. The proportion of pupils is only available for all pupils. It is not shown for pupil characteristics.";

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableKs2ProgressNumberOfPupils);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task FlagOff_ReturnsNotFound()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableKs2ProgressNumberOfPupils, false);

        await Fixture.RequestPageAsync(Path, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("ks2-progress-current-year-number-of-pupils", 1)]
    [InlineData("ks2-progress-year-by-year-number-of-pupils", 3)]
    [InlineData("ks2-progress-table-number-of-pupils", 3)]
    [InlineData("ks2-progress-top-performers-number-of-pupils", 1)]
    public async Task SchoolPage_ShowsCollapsedDetailsWithYearColumns(string id, int yearColumns)
    {
        var page = await Fixture.RequestPageAsync(Path);

        var details = page.ElementWithTestIdShouldExist(id);
        details.TagName.Should().BeEquivalentTo("details");
        details.HasAttribute("open").Should().BeFalse();
        details.QuerySelector(".govuk-details__summary-text")!.TextContent.Trim().Should().Be("Number of pupils");
        details.QuerySelector("p")!.TextContent.Trim().Should().Be(Hint);

        var headers = details.QuerySelectorAll("thead th").Select(h => h.TextContent.Trim()).ToList();
        headers.Take(2).Should().Equal("School(s)", "Pupils");
        headers.Skip(2).Should().HaveCount(yearColumns);
        headers.Last().Should().Be("2024 to 2025");
    }

    [Fact]
    public async Task SchoolPage_ShowsOnlyTheUsersSchoolUnderTheCharts()
    {
        var page = await Fixture.RequestPageAsync(Path);

        SchoolNames(page, "ks2-progress-current-year-number-of-pupils").Should().Equal("Example Primary School");
        SchoolNames(page, "ks2-progress-table-number-of-pupils").Should().Equal("Example Primary School");
    }

    [Fact]
    public async Task SchoolPage_TopPerformersAreInRankOrder()
    {
        var page = await Fixture.RequestPageAsync(Path);

        SchoolNames(page, "ks2-progress-top-performers-number-of-pupils").Should().Equal(
            "Example Top Primary School",
            "Example Second Primary School",
            "Example Primary School",
            "Example Fourth Primary School",
            "Example Fifth Primary School");
    }

    [Fact]
    public async Task ComparisonPage_ShowsBothSchoolsAndNoTopPerformers()
    {
        var page = await Fixture.RequestPageAsync($"{Path}?page=comparison");

        SchoolNames(page, "ks2-progress-current-year-number-of-pupils")
            .Should().Equal("Example Primary School", "Example Similar Primary School");
        page.QuerySelector("[data-testid='ks2-progress-top-performers-number-of-pupils']").Should().BeNull();
    }

    [Fact]
    public async Task EachSchoolHasEligiblePupilsAndProportionRows()
    {
        var page = await Fixture.RequestPageAsync(Path);

        var rowLabels = page.ElementWithTestIdShouldExist("ks2-progress-table-number-of-pupils")
            .QuerySelectorAll("tbody th[scope='row']")
            .Select(th => th.TextContent.Trim());

        rowLabels.Should().Equal("Eligible pupils at school", "Proportion of pupils included in the measure");
    }

    [Fact]
    public async Task AllPupils_ShowsProportionAsWholePercentage()
    {
        var page = await Fixture.RequestPageAsync(Path);

        ProportionCells(page).Should().OnlyContain(v => System.Text.RegularExpressions.Regex.IsMatch(v, @"^\d{1,3}%$"));
    }

    [Fact]
    public async Task PupilCharacteristic_ShowsProportionAsNotApplicable()
    {
        var page = await Fixture.RequestPageAsync($"{Path}?ks2-progress-characteristic=dis");

        ProportionCells(page).Should().OnlyContain(v => v == "N/A");
    }

    private static IEnumerable<string> SchoolNames(IDocument page, string id) =>
        page.ElementWithTestIdShouldExist(id)
            .QuerySelectorAll("tbody th[scope='rowgroup']")
            .Select(th => th.TextContent.Trim());

    private static IEnumerable<string> ProportionCells(IDocument page) =>
        page.ElementWithTestIdShouldExist("ks2-progress-table-number-of-pupils")
            .QuerySelectorAll("tbody tr:nth-child(2) td")
            .Select(td => td.TextContent.Trim());
}
