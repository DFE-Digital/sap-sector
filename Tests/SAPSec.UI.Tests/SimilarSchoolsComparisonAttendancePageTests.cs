using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using System.Text.RegularExpressions;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SimilarSchoolsComparisonAttendancePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string Path = "/school/108088/view-similar-schools/137621/Attendance";

    [Fact]
    public async Task AttendanceComparison_LoadsWithExpectedDefaults()
    {
        var response = await Page.GotoAsync(Path);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Attendance measures" })).ToBeVisibleAsync();

        var absenceType = Page.Locator("#attendanceAbsenceType");
        await Expect(absenceType).ToHaveValueAsync("overall");

        await Expect(Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-three-year-average']")).ToBeVisibleAsync();
        await Expect(Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-year-by-year']")).ToBeVisibleAsync();
        await Expect(Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-table']")).ToBeVisibleAsync();

        await Expect(Page.Locator(".govuk-tabs__list-item--selected .govuk-tabs__tab")).ToHaveTextAsync("3-year average");
    }

    [Fact]
    public async Task AttendanceComparison_TabsSwitchByClickAndKeyboard()
    {
        await Page.GotoAsync(Path);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-year-by-year']").ClickAsync();
        await Expect(Page.Locator(".govuk-tabs__list-item--selected .govuk-tabs__tab")).ToHaveTextAsync("Year by year");
        await Expect(Page.Locator("#attendance-year-by-year")).Not.ToHaveClassAsync(new Regex("govuk-tabs__panel--hidden"));

        var tableTab = Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-table']");
        await tableTab.FocusAsync();
        await tableTab.PressAsync("Space");
        await Expect(Page.Locator(".govuk-tabs__list-item--selected .govuk-tabs__tab")).ToHaveTextAsync("Table");
        await Expect(Page.Locator("#attendance-table")).Not.ToHaveClassAsync(new Regex("govuk-tabs__panel--hidden"));
        await Expect(Page.Locator("#attendance-year-by-year")).ToHaveClassAsync(new Regex("govuk-tabs__panel--hidden"));
    }
}
