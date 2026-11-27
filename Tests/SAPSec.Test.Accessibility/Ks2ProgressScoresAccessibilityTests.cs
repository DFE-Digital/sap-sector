using Deque.AxeCore.Playwright;
using FluentAssertions;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class Ks2ProgressScoresAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    [Theory]
    [MemberData(nameof(Pages))]
    public async Task Ks2ProgressScores_WithDetailsExpanded_PassAxeAccessibilityChecks(string path)
    {
        await NavigateTo(path);

        // Expand the progress scores information and every 'Number of pupils' component so they are checked too.
        await Page.EvaluateAsync("() => document.querySelectorAll('[data-testid=\"progress-rwm-details\"], details.app-number-of-pupils').forEach(d => d.open = true)");

        var results = await Page.RunAxe();
        var criticalViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        criticalViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", criticalViolations.Select(v => v.Description))}");
    }

    public static TheoryData<string> Pages() => new()
    {
        Routes.PrimarySchool("101193").KS2,
        $"{Routes.PrimarySchool("101193").KS2}?progress-subject=m",
        Routes.AllThroughSchool("100171").KS2,
        Routes.PrimarySchool("101193").Comparison("109635").Ks2,
    };
}
