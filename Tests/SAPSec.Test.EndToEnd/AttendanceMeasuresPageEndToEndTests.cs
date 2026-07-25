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
public class AttendanceMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string UrlPattern = @"\d{6}";
    private const string AttendanceMeasuresHeaderText = "Attendance";

    private const string Urn = "101206";
    private static readonly Routes.Primary PrimarySchoolRoute = Routes.PrimarySchool(Urn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(Urn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.Overview);
        await Page.GetByText("Attendance", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.Attendance);
    }

    [Fact]
    public async Task Attendance_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(AttendanceMeasuresHeaderText);
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
    public async Task Attendance_ViewTableView()
    {
        var section = await GetSection(AttendanceMeasuresHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var previous2 = await table.GetTableColumnAsync("2022 to 2023");
        await Expect(previous2).ToBePercentageValuesHavingCount(3);

        var previous = await table.GetTableColumnAsync("2023 to 2024");
        await Expect(previous).ToBePercentageValuesHavingCount(3);

        var current = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(current).ToBePercentageValuesHavingCount(3);
    }

    [Fact]
    public async Task Attendance_ChangeAbsenceTypeFilters()
    {
        var section = await GetSection(AttendanceMeasuresHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> absenceTypeValues = [];

        absenceTypeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var absenceType in new[] { "Overall absence", "Persistent absence" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Type of absence" }).SelectOptionAsync(absenceType);
            await table.WaitForDomToStopChanging();

            absenceTypeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        absenceTypeValues.Should().AllBeDifferent();
    }

    private async Task<ILocator> GetSection(string headerText)
    {
        var section = Page.GetByLabel(AttendanceMeasuresHeaderText);
        await Expect(section).ToBeVisibleAsync();

        return section;
    }
}