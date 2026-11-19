using Deque.AxeCore.Playwright;
using FluentAssertions;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Controllers;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class Ks2ProgressNumberOfPupilsAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    [Theory]
    [MemberData(nameof(Pages))]
    public async Task NumberOfPupilsExample_PassesAxeAccessibilityChecks(string path)
    {
        await NavigateTo(path);

        // Expand every 'Number of pupils' component so the tables are checked as well.
        await Page.EvaluateAsync("() => document.querySelectorAll('details.app-number-of-pupils').forEach(d => d.open = true)");

        var results = await Page.RunAxe();
        var criticalViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        criticalViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", criticalViolations.Select(v => v.Description))}");
    }

    public static TheoryData<string> Pages() => new()
    {
        ComponentExamplesController.Ks2ProgressNumberOfPupilsPath,
        $"{ComponentExamplesController.Ks2ProgressNumberOfPupilsPath}?page=comparison",
        $"{ComponentExamplesController.Ks2ProgressNumberOfPupilsPath}?ks2-progress-characteristic=dis"
    };
}
