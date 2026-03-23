using Deque.AxeCore.Playwright;
using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests.AccessibilityTests;

[Collection("UITestsCollection")]
public class SchoolAttendanceAccessibilityTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string AttendancePagePath = "/school/145327/attendance";

    [Fact]
    public async Task Attendance_PassesAxeAccessibilityChecks()
    {
        await Page.GotoAsync(AttendancePagePath);
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Page.WaitForSelectorAsync("main");

        var results = await Page.RunAxe();

        var criticalViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        criticalViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", criticalViolations.Select(v => v.Description))}");
    }
}
