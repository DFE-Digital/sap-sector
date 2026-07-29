using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using System.Text.RegularExpressions;
using Xunit;

namespace SAPSec.Test.EndToEnd;

[Collection("EndToEndTestsCollection")]
public class Ks2PerformanceMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string UrlPattern = @"\d{6}";
    private const string MeetingExpectedStandardHeaderText = "Meeting expected standard in reading, writing and maths";
    private const string ReadingScaledScoreHeaderText = "Average scaled score in reading";
    private const string MeetingExpectedStandardGpsHeaderText = "Meeting expected standard in grammar, punctuation and spelling";
    private const string AchievedHigherStandardGpsHeaderText = "Achieved a higher standard in grammar, punctuation and spelling";

    private const string Urn = "101206";
    private static readonly Routes.Primary PrimarySchoolRoute = Routes.PrimarySchool(Urn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(Urn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.Overview);
        await Page.GetByText("KS2", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.KS2);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        var panel = section.GetByRole(AriaRole.Tabpanel);

        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2024 to 2025" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });

        var showYearByYearButton = section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" });
        var showCurrentYearButton = section.GetByRole(AriaRole.Button, new() { Name = "Show 2024 to 2025" });

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await Expect(showYearByYearButton).ToBeVisibleAsync();
        await Expect(showCurrentYearButton).ToBeHiddenAsync();

        await showYearByYearButton.ClickAsync();

        await Expect(currentYearHeader).ToBeHiddenAsync();
        await Expect(yearByYearHeader).ToBeVisibleAsync();

        await Expect(showCurrentYearButton).ToBeVisibleAsync();
        await Expect(showYearByYearButton).ToBeHiddenAsync();

        await showCurrentYearButton.ClickAsync();

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await Expect(showYearByYearButton).ToBeVisibleAsync();
        await Expect(showCurrentYearButton).ToBeHiddenAsync();
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        var topPerfomersTab = section.GetByRole(AriaRole.Tab, new() { Name = "Top performers" });
        await topPerfomersTab.ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var values = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(values).ToBePercentageValuesHavingCount(3);

        var schools = await table.GetTableColumnAsync("School");
        await Expect(schools).ToHaveCountAsync(3);

        var schoolLinks = schools.GetByRole(AriaRole.Link);
        await schoolLinks.Nth(0).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(PrimarySchoolRoute.SimilarSchoolComparison(UrlPattern)));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ViewTableView()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var previous2 = await table.GetTableColumnAsync("2022 to 2023");
        await Expect(previous2).ToBePercentageValuesHavingCount(4);

        var previous = await table.GetTableColumnAsync("2023 to 2024");
        await Expect(previous).ToBePercentageValuesHavingCount(4);

        var current = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(current).ToBePercentageValuesHavingCount(4);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ChangeSubjectFilters()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> subjectValues = [];

        subjectValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Reading", "Writing", "Maths" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Subject" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            subjectValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        subjectValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task AverageScaledScoreReading_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(ReadingScaledScoreHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2024 to 2025" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" }).ClickAsync();

        await Expect(currentYearHeader).ToBeHiddenAsync();
        await Expect(yearByYearHeader).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AverageScaledScoreReading_ViewTableView()
    {
        var section = await GetSection(ReadingScaledScoreHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        foreach (var heading in new[] { "2022 to 2023", "2023 to 2024", "2024 to 2025" })
        {
            var values = await table.GetTableColumnAsync(heading);
            await Expect(values).ToHaveCountAsync(4);
            (await values.AllTrimmedTextContentsAsync()).Should().AllSatisfy(v =>
                (v == "No available data" || decimal.TryParse(v, out _)).Should().BeTrue());
        }
    }

    [Fact]
    public async Task AverageScaledScoreReading_ViewTopPerfomers()
    {
        var section = await GetSection(ReadingScaledScoreHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Top performers" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var values = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(values).ToHaveCountAsync(3);
        (await values.AllTrimmedTextContentsAsync()).Should().AllSatisfy(v => decimal.TryParse(v, out _).Should().BeTrue());
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(MeetingExpectedStandardGpsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2024 to 2025" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" }).ClickAsync();

        await Expect(currentYearHeader).ToBeHiddenAsync();
        await Expect(yearByYearHeader).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ViewTableView()
    {
        var section = await GetSection(MeetingExpectedStandardGpsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var current = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(current).ToBePercentageValuesHavingCount(4);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(MeetingExpectedStandardGpsHeaderText);
        var topPerfomersTab = section.GetByRole(AriaRole.Tab, new() { Name = "Top performers" });
        await topPerfomersTab.ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School");
        await Expect(schools).ToHaveCountAsync(3);

        await schools.GetByRole(AriaRole.Link).Nth(0).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(PrimarySchoolRoute.SimilarSchoolComparison(UrlPattern)));
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(AchievedHigherStandardGpsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2024 to 2025" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" }).ClickAsync();

        await Expect(currentYearHeader).ToBeHiddenAsync();
        await Expect(yearByYearHeader).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ViewTableView()
    {
        var section = await GetSection(AchievedHigherStandardGpsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var current = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(current).ToBePercentageValuesHavingCount(4);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(AchievedHigherStandardGpsHeaderText);
        var topPerfomersTab = section.GetByRole(AriaRole.Tab, new() { Name = "Top performers" });
        await topPerfomersTab.ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School");
        await Expect(schools).ToHaveCountAsync(3);

        await schools.GetByRole(AriaRole.Link).Nth(0).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(PrimarySchoolRoute.SimilarSchoolComparison(UrlPattern)));
    }

    private async Task<ILocator> GetSection(string headerText)
    {
        var section = Page.GetByLabel(headerText);
        await Expect(section).ToBeVisibleAsync();

        return section;
    }
}
