using Deque.AxeCore.Playwright;
using FluentAssertions;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class Ks4CoreSubjectsAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    [Theory]
    [MemberData(nameof(Ks4CoreSubjectsPages))]
    public async Task Ks4CoreSubjectsPages_PassAxeAccessibilityChecks(string path)
    {
        await NavigateTo(path);

        var results = await Page.RunAxe();
        var criticalViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        criticalViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", criticalViolations.Select(v => v.Description))}");
    }

    public static TheoryData<string> Ks4CoreSubjectsPages() => new()
    {
        Routes.AllThroughSchool("100171").KS4CoreSubjects,
        Routes.SecondarySchool("100182").KS4CoreSubjects,
        Routes.SecondarySchool("100182").Comparison("136555").KS4CoreSubjects,
    };
}
