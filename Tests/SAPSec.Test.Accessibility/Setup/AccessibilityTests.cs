using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using SAPSec.Test.EndToEnd.Setup;

namespace SAPSec.Test.Accessibility.Setup;

public abstract class AccessibilityTests : PageTest
{
    private readonly AccessibilityTestsFixture _fixture;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected AccessibilityTests(AccessibilityTestsFixture fixture)
    {
        _fixture = fixture;

        // Run in headed mode when debugging
        if (System.Diagnostics.Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("HEADED", "1");
        }
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = _fixture.BaseUrl.TrimEnd('/'),
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1280, Height = 720 },
            Locale = "en-GB",
            TimezoneId = "Europe/London",
            JavaScriptEnabled = true,
        };
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Page.SetDefaultTimeout((float)TimeSpan.FromSeconds(10).TotalMilliseconds);
        Page.SetDefaultNavigationTimeout((float)TimeSpan.FromSeconds(30).TotalMilliseconds);
    }

    protected async Task NavigateTo(string path)
    {
        var response = await Page.GotoAsync(path);

        response.Should().NotBeNull();
        response.Status.Should().Be(200);
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Page.WaitForSelectorAsync("main");
    }

    protected async Task CurrentPageShouldNowBe(string path)
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForURLAsync($"**{path}");
    }

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

    private static string FormatViolations(AxeResultItem[] violations)
    {
        if (violations.Length == 0) return "None";

        return string.Join("\n", violations.Select(v =>
            $"- [{v.Impact?.ToUpper()}] {v.Id}: {v.Description} " +
            $"(Affected: {v.Nodes.Length} elements)"));
    }

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
}