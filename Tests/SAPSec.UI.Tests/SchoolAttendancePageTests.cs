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
        await Expect(Page.Locator("main")).ToContainTextAsync("local averages");
        await Expect(Page.Locator("main")).ToContainTextAsync("national averages");
        await Expect(Page.Locator("main")).ToContainTextAsync("Monitor your school attendance service");
        await Expect(Page.Locator("#school-attendance-three-year-chart")).ToBeVisibleAsync();
        await Expect(Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-year-by-year']")).ToBeVisibleAsync();
        await Expect(Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-table']")).ToBeVisibleAsync();
        await Expect(Page.Locator("#attendanceAbsenceType")).ToHaveValueAsync("overall");
    }

    [Fact]
    public async Task Attendance_SidebarShowsAttendanceAsActiveNonNavigableItem()
    {
        await NavigateToAttendanceAsync();

        var activeItem = Page.Locator(".app-side-navigation__item--selected");
        await Expect(activeItem).ToContainTextAsync("Attendance");
        (await activeItem.Locator("a").CountAsync()).Should().Be(0);
        await Expect(activeItem.Locator("span[aria-current='page']")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Attendance_ExternalLinksOpenInNewTab()
    {
        await NavigateToAttendanceAsync();

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "View your education data (VYED)" }))
            .ToHaveAttributeAsync("target", "_blank");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "you can follow this guidance" }))
            .ToHaveAttributeAsync("target", "_blank");
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
        await Expect(selectedView).ToHaveTextAsync(new Regex("3-year average"));
    }

    [Fact]
    public async Task Attendance_ChartOnlyVisibleWhenThreeYearAverageTabIsActive()
    {
        await NavigateToAttendanceAsync();

        await Expect(Page.Locator("#attendance-three-year-average")).Not.ToHaveClassAsync(new Regex("govuk-tabs__panel--hidden"));
        await Expect(Page.Locator("#school-attendance-three-year-chart")).ToBeVisibleAsync();

        await Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-year-by-year']").ClickAsync();

        await Expect(Page.Locator("#attendance-year-by-year")).Not.ToHaveClassAsync(new Regex("govuk-tabs__panel--hidden"));
        await Expect(Page.Locator("#attendance-three-year-average")).ToHaveClassAsync(new Regex("govuk-tabs__panel--hidden"));
    }

    [Fact]
    public async Task Attendance_TabsSwitchToYearByYearAndTable()
    {
        await NavigateToAttendanceAsync();

        await Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-year-by-year']").ClickAsync();
        await Expect(Page.Locator("#school-attendance-year-by-year-chart")).ToBeVisibleAsync();

        var tableTab = Page.Locator(".app-attendance-tabs .govuk-tabs__tab[href='#attendance-table']");
        await tableTab.ClickAsync();
        await Expect(Page.Locator("#attendance-table .govuk-table")).ToBeVisibleAsync();
    }
}
