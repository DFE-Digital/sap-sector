using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Primary;

[Collection("EndToEndTestsCollection")]
public class ComparisonKs2PerformanceMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string MeetingExpectedStandardHeaderText = "Meeting expected standard in reading, writing and maths";
    private const string AchievedHigherStandardHeaderText = "Achieved a higher standard in reading, writing and maths";
    private const string ReadingScaledScoreHeaderText = "Average scaled score in reading";
    private const string MathsScaledScoreHeaderText = "Average scaled score in maths";
    private const string MeetingExpectedStandardGpsHeaderText = "Meeting expected standard in grammar, punctuation and spelling";
    private const string AchievedHigherStandardGpsHeaderText = "Achieved a higher standard in grammar, punctuation and spelling";

    private const string Urn = "101206";
    private static readonly Routes.Primary PrimarySchoolRoute = Routes.PrimarySchool(Urn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(PrimarySchoolRoute.ViewSimilarSchools);

        await Page.Locator(".app-school-result a").First.ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "KS2", Exact = true }).ClickAsync();
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_DetailsToggle_IsCollapsedByDefaultAndExpands()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        var details = section.Locator(".app-measure-details");

        await Expect(details).Not.ToHaveAttributeAsync("open", "");
        await details.Locator("summary").ClickAsync();
        await Expect(details).ToHaveAttributeAsync("open", "");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ViewTableView()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School(s)");
        await Expect(schools).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ChangeSubjectFilter_UpdatesTableValues()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> subjectValues = [];
        subjectValues.Add(await table.GetCells().AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Reading", "Writing", "Maths" })
        {
            await section.GetByLabel("Subject").SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            subjectValues.Add(await table.GetCells().AllTrimmedTextContentsAsync());
        }

        subjectValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ToggleBetweenYearByYearAndCurrentYearView()
    {
        await AssertCanToggleBetweenYearByYearAndCurrentYearView(MeetingExpectedStandardHeaderText);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_ViewTableView()
    {
        var section = await GetSection(AchievedHigherStandardHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School(s)");
        await Expect(schools).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_ChangeSubjectFilter_UpdatesTableValues()
    {
        var section = await GetSection(AchievedHigherStandardHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> subjectValues = [];
        subjectValues.Add(await table.GetCells().AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Reading", "Writing", "Maths" })
        {
            await section.GetByLabel("Subject").SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            subjectValues.Add(await table.GetCells().AllTrimmedTextContentsAsync());
        }

        subjectValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_ToggleBetweenYearByYearAndCurrentYearView()
    {
        await AssertCanToggleBetweenYearByYearAndCurrentYearView(AchievedHigherStandardHeaderText);
    }

    [Fact]
    public async Task AverageScaledScoreReading_DetailsToggle_IsCollapsedByDefaultAndExpands()
    {
        var section = await GetSection(ReadingScaledScoreHeaderText);
        var details = section.Locator(".app-measure-details");

        await Expect(details).Not.ToHaveAttributeAsync("open", "");
        await details.Locator("summary").ClickAsync();
        await Expect(details).ToHaveAttributeAsync("open", "");
    }

    [Fact]
    public async Task AverageScaledScoreReading_ViewTableView()
    {
        var section = await GetSection(ReadingScaledScoreHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School(s)");
        await Expect(schools).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task AverageScaledScoreReading_ToggleBetweenYearByYearAndCurrentYearView()
    {
        await AssertCanToggleBetweenYearByYearAndCurrentYearView(ReadingScaledScoreHeaderText);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_DetailsToggle_IsCollapsedByDefaultAndExpands()
    {
        var section = await GetSection(MathsScaledScoreHeaderText);
        var details = section.Locator(".app-measure-details");

        await Expect(details).Not.ToHaveAttributeAsync("open", "");
        await details.Locator("summary").ClickAsync();
        await Expect(details).ToHaveAttributeAsync("open", "");
    }

    [Fact]
    public async Task AverageScaledScoreMaths_ViewTableView()
    {
        var section = await GetSection(MathsScaledScoreHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School(s)");
        await Expect(schools).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_ToggleBetweenYearByYearAndCurrentYearView()
    {
        await AssertCanToggleBetweenYearByYearAndCurrentYearView(MathsScaledScoreHeaderText);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_DetailsToggle_IsCollapsedByDefaultAndExpands()
    {
        var section = await GetSection(MeetingExpectedStandardGpsHeaderText);
        var details = section.Locator(".app-measure-details");

        await Expect(details).Not.ToHaveAttributeAsync("open", "");
        await details.Locator("summary").ClickAsync();
        await Expect(details).ToHaveAttributeAsync("open", "");
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ViewTableView()
    {
        var section = await GetSection(MeetingExpectedStandardGpsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School(s)");
        await Expect(schools).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ToggleBetweenYearByYearAndCurrentYearView()
    {
        await AssertCanToggleBetweenYearByYearAndCurrentYearView(MeetingExpectedStandardGpsHeaderText);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_DetailsToggle_IsCollapsedByDefaultAndExpands()
    {
        var section = await GetSection(AchievedHigherStandardGpsHeaderText);
        var details = section.Locator(".app-measure-details");

        await Expect(details).Not.ToHaveAttributeAsync("open", "");
        await details.Locator("summary").ClickAsync();
        await Expect(details).ToHaveAttributeAsync("open", "");
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ViewTableView()
    {
        var section = await GetSection(AchievedHigherStandardGpsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School(s)");
        await Expect(schools).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ToggleBetweenYearByYearAndCurrentYearView()
    {
        await AssertCanToggleBetweenYearByYearAndCurrentYearView(AchievedHigherStandardGpsHeaderText);
    }

    private async Task<ILocator> GetSection(string headerText)
    {
        var section = Page.GetByLabel(headerText);
        await Expect(section).ToBeVisibleAsync();

        return section;
    }

    private async Task AssertCanToggleBetweenYearByYearAndCurrentYearView(string headerText)
    {
        var section = await GetSection(headerText);
        var panel = section.GetByRole(AriaRole.Tabpanel);

        await section.GetByRole(AriaRole.Tab, new() { Name = "3-year average chart" }).ClickAsync();

        var currentYearPanel = panel.Locator("[data-content-toggle-name=\"2024 to 2025\"]");
        var yearByYearPanel = panel.Locator("[data-content-toggle-name=\"Year by year\"]");
        var toggleButton = section.Locator(".app-content-toggle__header button[type=\"button\"]");

        await Expect(currentYearPanel).ToBeVisibleAsync();
        await Expect(yearByYearPanel).ToBeHiddenAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "false");

        await toggleButton.ClickAsync();

        await Expect(currentYearPanel).ToBeHiddenAsync();
        await Expect(yearByYearPanel).ToBeVisibleAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "true");

        await toggleButton.ClickAsync();

        await Expect(currentYearPanel).ToBeVisibleAsync();
        await Expect(yearByYearPanel).ToBeHiddenAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "false");
    }
}
