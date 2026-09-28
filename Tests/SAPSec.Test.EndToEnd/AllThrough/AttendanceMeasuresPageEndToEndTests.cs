using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.AllThrough;

[Collection("EndToEndTestsCollection")]
public class AttendanceMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string PrimaryHeaderText = "Primary attendance";
    private const string SecondaryHeaderText = "Secondary attendance";

    private const string Urn = "100171";
    private static readonly Routes.AllThrough AllThroughSchoolRoute = Routes.AllThroughSchool(Urn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(Urn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(AllThroughSchoolRoute.Overview);
        await Page.GetByText("Attendance", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(AllThroughSchoolRoute.Attendance);
    }

    [Fact]
    public async Task Attendance_ShowsPrimaryAndSecondarySectionsWithoutTopPerformers()
    {
        var primary = await GetSection(PrimaryHeaderText);
        var secondary = await GetSection(SecondaryHeaderText);

        foreach (var section in new[] { primary, secondary })
        {
            await Expect(section.GetByRole(AriaRole.Tab, new() { Name = "Charts" })).ToBeVisibleAsync();
            await Expect(section.GetByRole(AriaRole.Tab, new() { Name = "Table" })).ToBeVisibleAsync();
            await Expect(section.GetByRole(AriaRole.Tab, new() { Name = "Top performers" })).ToHaveCountAsync(0);
        }
    }

    [Theory]
    [InlineData(PrimaryHeaderText)]
    [InlineData(SecondaryHeaderText)]
    public async Task Attendance_ToggleBetweenYearByYearAndCurrentYearView(string headerText)
    {
        var section = await GetSection(headerText);
        var panel = section.GetByRole(AriaRole.Tabpanel);

        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

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

    [Theory]
    [InlineData(PrimaryHeaderText, "Local authority primary schools average", "Primary schools in England average")]
    [InlineData(SecondaryHeaderText, "Local authority secondary schools average", "Secondary schools in England average")]
    public async Task Attendance_ViewTableView(string headerText, string laLabel, string englandLabel)
    {
        var section = await GetSection(headerText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var schools = await table.GetTableColumnAsync("School(s)");
        var schoolLabels = await schools.AllTrimmedTextContentsAsync();
        schoolLabels.Should().Contain([laLabel, englandLabel]);

        foreach (var year in new[] { "2022 to 2023", "2023 to 2024", "2024 to 2025" })
        {
            var column = await table.GetTableColumnAsync(year);
            await Expect(column).ToBePercentageValuesHavingCount(3);
        }
    }

    [Fact]
    public async Task Attendance_FilteringPrimarySection_DoesNotChangeSecondarySection()
    {
        var primary = await GetSection(PrimaryHeaderText);
        var secondary = await GetSection(SecondaryHeaderText);

        await primary.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();
        await secondary.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var primarySchoolLatest = LatestYearSchoolCell(primary);
        var secondarySchoolLatest = LatestYearSchoolCell(secondary);

        var primaryBefore = (await primarySchoolLatest.InnerTextAsync()).Trim();
        var secondaryBefore = (await secondarySchoolLatest.InnerTextAsync()).Trim();

        await primary.GetByLabel("Type of absence").SelectOptionAsync(new SelectOptionValue { Label = "Persistent absence" });

        await Expect(primarySchoolLatest).Not.ToHaveTextAsync(primaryBefore);
        await Expect(secondarySchoolLatest).ToHaveTextAsync(secondaryBefore);
        await Expect(secondary.GetByLabel("Type of absence")).ToHaveValueAsync("o");
    }

    [Fact]
    public async Task Attendance_FilteringSecondarySection_DoesNotChangePrimarySection()
    {
        var primary = await GetSection(PrimaryHeaderText);
        var secondary = await GetSection(SecondaryHeaderText);

        await primary.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();
        await secondary.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var primarySchoolLatest = LatestYearSchoolCell(primary);
        var secondarySchoolLatest = LatestYearSchoolCell(secondary);

        var primaryBefore = (await primarySchoolLatest.InnerTextAsync()).Trim();
        var secondaryBefore = (await secondarySchoolLatest.InnerTextAsync()).Trim();

        await secondary.GetByLabel("Pupil characteristic").SelectOptionAsync(new SelectOptionValue { Label = "Boys" });

        await Expect(secondarySchoolLatest).Not.ToHaveTextAsync(secondaryBefore);
        await Expect(primarySchoolLatest).ToHaveTextAsync(primaryBefore);
        await Expect(primary.GetByLabel("Pupil characteristic")).ToHaveValueAsync("tot");
    }

    private static ILocator LatestYearSchoolCell(ILocator section) =>
        section.GetByRole(AriaRole.Table).Locator("tbody tr").First.Locator("td").Last;

    private async Task<ILocator> GetSection(string headerText)
    {
        var section = Page.GetByLabel(headerText, new() { Exact = true });
        await Expect(section).ToBeVisibleAsync();

        return section;
    }
}
