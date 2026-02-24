using Deque.AxeCore.Playwright;
using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests.AccessibilityTests;

[Collection("UITestsCollection")]
public class SimilarSchoolsAccessibilityTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string SimilarSchoolsPath = "/school/108088/view-similar-schools";

    private async Task NavigateToPageAsync()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Page.WaitForSelectorAsync("main");
    }

    [Fact]
    public async Task SimilarSchools_PassesAxeAccessibilityChecks()
    {
        await NavigateToPageAsync();

        var results = await Page.RunAxe();
        var criticalViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        criticalViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", criticalViolations.Select(v => v.Description))}");
    }

    [Fact]
    public async Task SimilarSchools_HasExpectedLandmarksAndHeadingStructure()
    {
        await NavigateToPageAsync();

        (await Page.Locator("main#main-content").CountAsync()).Should().Be(1);
        (await Page.Locator("header").CountAsync()).Should().BeGreaterThan(0);
        (await Page.Locator("footer").CountAsync()).Should().Be(1);
        (await Page.Locator("h1").CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SimilarSchools_FilterFormControls_HaveLabels()
    {
        await NavigateToPageAsync();

        var unlabeledControls = await Page.EvaluateAsync<int>(@"
            () => {
                const form = document.querySelector('#similar-schools-filter-form');
                if (!form) return -1;

                const controls = form.querySelectorAll('input, select, textarea');
                const isHidden = (el) =>
                    el.type === 'hidden' || el.closest('[hidden], [aria-hidden=""true""]');

                let unlabeled = 0;
                controls.forEach(control => {
                    if (isHidden(control)) return;
                    const id = control.getAttribute('id');
                    const hasLabel = (id && document.querySelector(`label[for=""${id}""]`)) ||
                        control.hasAttribute('aria-label') ||
                        control.hasAttribute('aria-labelledby');
                    if (!hasLabel) unlabeled++;
                });

                return unlabeled;
            }
        ");

        unlabeledControls.Should().Be(0, "All visible form controls should have accessible labels");
    }

    [Fact]
    public async Task SimilarSchools_MapRegion_HasAccessibleName()
    {
        await NavigateToPageAsync();

        var map = Page.Locator("#map");
        (await map.CountAsync()).Should().Be(1);
        (await map.GetAttributeAsync("role")).Should().Be("region");
        (await map.GetAttributeAsync("aria-label")).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SimilarSchools_FilterSections_ExposeExpandCollapseState()
    {
        await NavigateToPageAsync();

        var toggles = Page.Locator(".app-filter-section__toggle");
        var count = await toggles.CountAsync();
        count.Should().BeGreaterThan(0);

        for (var i = 0; i < Math.Min(count, 5); i++)
        {
            var toggle = toggles.Nth(i);
            var controls = await toggle.GetAttributeAsync("aria-controls");
            var expanded = await toggle.GetAttributeAsync("aria-expanded");

            controls.Should().NotBeNullOrWhiteSpace();
            (expanded == "true" || expanded == "false").Should().BeTrue();

            var controlledCount = await Page.Locator($"#{controls}").CountAsync();
            controlledCount.Should().Be(1, $"Filter toggle {i + 1} should reference a valid panel");
        }
    }

    [Fact]
    public async Task SimilarSchools_Pagination_CurrentPageUsesAriaCurrent()
    {
        await NavigateToPageAsync();

        var pagination = Page.Locator("nav.govuk-pagination");
        var paginationCount = await pagination.CountAsync();

        if (paginationCount == 0)
        {
            return;
        }

        (await pagination.GetAttributeAsync("aria-label")).Should().NotBeNullOrWhiteSpace();

        var currentPageLink = pagination.Locator("a[aria-current='page']");
        (await currentPageLink.CountAsync()).Should().Be(1);
    }
}
