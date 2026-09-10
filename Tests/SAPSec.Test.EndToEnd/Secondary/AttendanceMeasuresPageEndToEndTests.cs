using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Secondary;

[Collection("EndToEndTestsCollection")]
public class AttendanceMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string AttendanceMeasuresHeaderText = "Attendance";

    private const string Urn = "100051";
    private static readonly Routes.Secondary SecondarySchoolRoute = Routes.SecondarySchool(Urn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(Urn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.Overview);
        await Page.GetByText("Attendance", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.Attendance);
    }

    [Fact]
    public async Task Attendance_ToggleBetweenYearByYearAndCurrentYearView()
    {
        await AssertCanToggleBetweenYearByYearAndCurrentYearView("2023 to 2024");
    }

    [Fact]
    public async Task Attendance_ViewTableView()
    {
        var section = await GetSection(AttendanceMeasuresHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var previous2 = await table.GetTableColumnAsync("2021 to 2022");
        await Expect(previous2).ToBePercentageValuesHavingCount(3);

        var previous = await table.GetTableColumnAsync("2022 to 2023");
        await Expect(previous).ToBePercentageValuesHavingCount(3);

        var current = await table.GetTableColumnAsync("2023 to 2024");
        await Expect(current).ToBePercentageValuesHavingCount(3);
    }

    private async Task<ILocator> GetSection(string headerText)
    {
        var section = Page.GetByLabel(AttendanceMeasuresHeaderText);
        await Expect(section).ToBeVisibleAsync();

        return section;
    }

    private async Task AssertCanToggleBetweenYearByYearAndCurrentYearView(string currentYearContentName)
    {
        var section = await GetSection(AttendanceMeasuresHeaderText);
        var panel = section.GetByRole(AriaRole.Tabpanel);

        await section.GetByRole(AriaRole.Tab, new() { Name = "3-year average chart" }).ClickAsync();

        var currentYearPanel = panel.Locator($"[data-content-toggle-name=\"{currentYearContentName}\"]");
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
