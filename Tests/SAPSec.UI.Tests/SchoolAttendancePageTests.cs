using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using System.Text.RegularExpressions;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SchoolAttendancePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string AttendancePagePath = "/school/145327/attendance";

    private async Task NavigateToAttendanceAsync()
    {
        await Page.GotoAsync(AttendancePagePath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private ILocator AttendanceTabs => Page.Locator(".app-attendance-tabs");

    private ILocator AttendanceToggleButton =>
        AttendanceTabs.GetByRole(AriaRole.Button, new() { Name = "Show year by year" });

    [Fact]
    public async Task Attendance_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(AttendancePagePath);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task Attendance_RendersExpectedShellAndChart()
    {
        await NavigateToAttendanceAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Attendance measures" })).ToBeVisibleAsync();
        await Expect(Page.Locator("main")).ToContainTextAsync("50 similar secondary phase schools (including all-throughs)");
        await Expect(Page.Locator("main")).ToContainTextAsync("the local authority average");
        await Expect(Page.Locator("main")).ToContainTextAsync("the national average");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "how DfE defines what a similar school is" }))
            .ToHaveAttributeAsync("href", "/school/145327/what-is-a-similar-school");
        await Expect(Page.Locator("#school-attendance-three-year-chart")).ToBeVisibleAsync();
        await Expect(Page.Locator("#school-attendance-three-year-chart")).ToHaveAttributeAsync("data-show-no-data-labels", "true");
        await Expect(Page.Locator("#school-attendance-three-year-chart")).ToHaveAttributeAsync("data-colors", "[\"#D53780\",\"#2a1950\",\"#2a1950\"]");
        await Expect(Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-charts']")).ToBeVisibleAsync();
        await Expect(Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-table']")).ToBeVisibleAsync();
        await Expect(Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-top-performers']")).ToBeVisibleAsync();
        await Expect(AttendanceTabs.GetByRole(AriaRole.Button, new() { Name = "Show year by year" })).ToBeVisibleAsync();
        await Expect(Page.Locator("label[for='attendanceAbsenceType']")).ToHaveClassAsync(new Regex("govuk-label--s"));
        await Expect(Page.Locator("#attendanceAbsenceType")).ToHaveValueAsync("overall");
    }

    [Fact]
    public async Task Attendance_SidebarShowsAttendanceAsActiveNavigableItem()
    {
        await NavigateToAttendanceAsync();

        var activeItem = Page.Locator(".app-side-navigation__item--selected");
        await Expect(activeItem).ToContainTextAsync("Attendance");
        var activeLink = activeItem.Locator("a.app-side-navigation__link--selected");
        await Expect(activeLink).ToBeVisibleAsync();
        await Expect(activeLink).ToHaveAttributeAsync("href", "/school/145327/attendance");
        await Expect(activeLink).ToHaveAttributeAsync("aria-current", "page");
    }

    [Fact]
    public async Task Attendance_ChangingAbsenceTypeUpdatesChartWithoutReload()
    {
        await NavigateToAttendanceAsync();

        var chart = Page.Locator("#school-attendance-three-year-chart");
        var initialDataset = await chart.EvaluateAsync<string>(@"
            el => JSON.stringify(window.Chart.getChart(el).data.datasets[0].data)
        ");

        await Page.SelectOptionAsync("#attendanceAbsenceType", "persistent");
        await Expect(Page.Locator("#attendanceAbsenceType")).ToHaveValueAsync("persistent");
        await Expect(chart).ToBeVisibleAsync();

        await Page.WaitForFunctionAsync(
            @"([selector, initial]) => {
                const canvas = document.querySelector(selector);
                const chart = canvas && window.Chart ? window.Chart.getChart(canvas) : null;
                if (!chart || !chart.data || !chart.data.datasets || !chart.data.datasets[0]) {
                    return false;
                }
                return JSON.stringify(chart.data.datasets[0].data) !== initial;
            }",
            new object[] { "#school-attendance-three-year-chart", initialDataset });

        var updatedDataset = await chart.EvaluateAsync<string>(@"
            el => JSON.stringify(window.Chart.getChart(el).data.datasets[0].data)
        ");

        updatedDataset.Should().NotBe(initialDataset);

        var selectedView = Page.Locator(".govuk-tabs__list-item--selected .govuk-tabs__tab");
        await Expect(selectedView).ToHaveTextAsync("Charts");
        await Expect(Page.Locator(".app-content-toggle__title")).ToHaveTextAsync("3-year average");
    }

    [Fact]
    public async Task Attendance_ChartToggleSwitchesBetweenThreeYearAverageAndYearByYear()
    {
        await NavigateToAttendanceAsync();

        await Expect(Page.Locator(".app-content-toggle__title")).ToHaveTextAsync("3-year average");
        await Expect(Page.Locator("#school-attendance-three-year-chart")).ToBeVisibleAsync();
        await Expect(Page.Locator("#attendance-year-by-year")).ToHaveAttributeAsync("hidden", "hidden");

        await AttendanceToggleButton.ClickAsync();

        await Expect(Page.Locator(".app-content-toggle__title")).ToHaveTextAsync("Year by year");
        await Expect(Page.Locator("#school-attendance-year-by-year-chart")).ToBeVisibleAsync();
        await Expect(Page.Locator("#attendance-three-year-average")).ToHaveAttributeAsync("hidden", "hidden");
    }

    [Fact]
    public async Task Attendance_TabsSwitchToYearByYearAndTable()
    {
        await NavigateToAttendanceAsync();

        await AttendanceToggleButton.ClickAsync();
        await Expect(Page.Locator("#school-attendance-year-by-year-chart")).ToBeVisibleAsync();
        await Expect(AttendanceTabs.GetByRole(AriaRole.Button, new() { Name = "Show 3-year average" })).ToBeVisibleAsync();

        var tableTab = Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-table']");
        await tableTab.ClickAsync();
        await Expect(Page.Locator("#attendance-table .govuk-table")).ToBeVisibleAsync();
        await Expect(Page.Locator("#attendance-table .govuk-table")).ToContainTextAsync("Local authority average");
        await Expect(Page.Locator("#attendance-table .govuk-table")).Not.ToContainTextAsync("Similar schools average");
    }
}
