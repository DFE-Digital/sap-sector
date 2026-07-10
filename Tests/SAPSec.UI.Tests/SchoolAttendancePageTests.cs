using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SchoolAttendancePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string PagePath = "/school/145327/attendance";

    [Fact]
    public async Task Attendance_ShowsOnlyChartsAndTableTabs_ForBothAbsenceTypes()
    {
        await Page.GotoAsync(PagePath, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Attendance measures" })).ToBeVisibleAsync();

        await AssertTabStateAsync();

        await Page.SelectOptionAsync("#attendanceAbsenceType", "persistent");
        await Page.WaitForTimeoutAsync(250);

        await AssertTabStateAsync();
    }

    private async Task AssertTabStateAsync()
    {
        var tabs = Page.Locator(".app-attendance-tabs .govuk-tabs__tab");
        await Expect(tabs).ToHaveCountAsync(2);

        var tabTexts = await tabs.AllTextContentsAsync();
        tabTexts.Should().Equal("Charts", "Table");

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Top performers" })).ToHaveCountAsync(0);
    }
}
