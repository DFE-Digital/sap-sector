using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests.Accessibility;

/// <summary>
/// Base class for accessibility tests using Axe-Core.
/// Provides helper methods for running WCAG compliance checks.
/// </summary>
public abstract class AccessibilityTestBase : BasePageTest
{
    protected AccessibilityTestBase(WebApplicationSetupFixture fixture) : base(fixture)
    {
    }

    #region Axe Analysis Helpers

    /// <summary>
    /// Runs full accessibility analysis on the current page.
    /// </summary>
    protected async Task<AxeResult> AnalyzePageAccessibility()
    {
        return await Page.RunAxe();
    }

    /// <summary>
    /// Runs accessibility analysis with specific WCAG tags.
    /// </summary>
    protected async Task<AxeResult> AnalyzePageAccessibility(params string[] wcagTags)
    {
        var options = new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions
            {
                Type = "tag",
                Values = wcagTags.ToList()
            }
        };

        return await Page.RunAxe(options);
    }

    /// <summary>
    /// Runs accessibility analysis on a specific element.
    /// </summary>
    protected async Task<AxeResult> AnalyzeElementAccessibility(string selector)
    {
        var locator = Page.Locator(selector);
        return await locator.RunAxe();
    }

    #endregion

    #region Assertion Helpers

    /// <summary>
    /// Asserts that the page has no accessibility violations.
    /// </summary>
    protected static void AssertNoViolations(AxeResult result)
    {
        var violations = result.Violations;

        violations.Should().BeEmpty(
            $"Expected no accessibility violations but found {violations.Length}: " +
            $"{FormatViolations(violations)}");
    }

    /// <summary>
    /// Asserts that the page has no critical or serious accessibility violations.
    /// </summary>
    protected static void AssertNoCriticalViolations(AxeResult result)
    {
        var criticalViolations = result.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToArray();

        criticalViolations.Should().BeEmpty(
            $"Expected no critical/serious violations but found {criticalViolations.Length}: " +
            $"{FormatViolations(criticalViolations)}");
    }

    /// <summary>
    /// Asserts that the page passes specific WCAG criteria.
    /// </summary>
    protected static void AssertPassesWcagCriteria(AxeResult result, string[] requiredPasses)
    {
        var passedRules = result.Passes.Select(p => p.Id).ToList();

        foreach (var criteria in requiredPasses)
        {
            passedRules.Should().Contain(criteria,
                $"Page should pass WCAG criteria: {criteria}");
        }
    }

    #endregion

    #region Formatting Helpers

    private static string FormatViolations(AxeResultItem[] violations)
    {
        if (violations.Length == 0) return "None";

        return string.Join("\n", violations.Select(v =>
            $"- [{v.Impact?.ToUpper()}] {v.Id}: {v.Description} " +
            $"(Affected: {v.Nodes.Length} elements)"));
    }

    #endregion

    #region WCAG Tag Constants

    protected static class WcagTags
    {
        // WCAG 2.1 Level A
        public const string Wcag2A = "wcag2a";

        // WCAG 2.1 Level AA (includes Level A)
        public const string Wcag2AA = "wcag2aa";

        // WCAG 2.1 Level AAA (includes Level A and AA)
        public const string Wcag2AAA = "wcag2aaa";

        // WCAG 2.2 Level A
        public const string Wcag22A = "wcag22a";

        // WCAG 2.2 Level AA
        public const string Wcag22AA = "wcag22aa";

        // Best Practices (not strictly WCAG)
        public const string BestPractice = "best-practice";

        // Section 508 compliance
        public const string Section508 = "section508";

        // Common tag combinations for UK Government
        public static readonly string[] GovUkRequired = { Wcag2AA, BestPractice };
    }

    #endregion
}