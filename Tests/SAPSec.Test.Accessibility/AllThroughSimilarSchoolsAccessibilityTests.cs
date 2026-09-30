using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class AllThroughSimilarSchoolsAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    private const string PhaseTabsSelector = ".app-phase-tabs .govuk-service-navigation__link";

    [Fact]
    public async Task PhaseTabs_AreKeyboardAccessibleAndStackOnMobile()
    {
        await NavigateTo(Routes.AllThroughSchool("100171").ViewSimilarSchools);

        var tabs = await WaitForPhaseTabs();
        await tabs.Nth(0).FocusAsync();

        (await Page.EvaluateAsync<bool>("() => document.activeElement?.textContent?.trim() === 'Primary'"))
            .Should().BeTrue();

        await tabs.Nth(1).FocusAsync();

        (await Page.EvaluateAsync<bool>("() => document.activeElement?.textContent?.trim() === 'Secondary'"))
            .Should().BeTrue();

        await Expect(tabs.Nth(0)).ToHaveAttributeAsync("aria-current", "page");
        (await tabs.Nth(1).GetAttributeAsync("aria-current")).Should().BeNull();

        await Page.SetViewportSizeAsync(375, 667);
        await NavigateTo(Routes.AllThroughSchool("100171").ViewSimilarSchools);

        tabs = await WaitForPhaseTabs();
        var primaryBox = await tabs.Nth(0).BoundingBoxAsync();
        var secondaryBox = await tabs.Nth(1).BoundingBoxAsync();

        primaryBox.Should().NotBeNull();
        secondaryBox.Should().NotBeNull();
        secondaryBox!.Y.Should().BeGreaterThan(primaryBox!.Y + primaryBox.Height - 1);
    }

    private async Task<ILocator> WaitForPhaseTabs()
    {
        var tabs = Page.Locator(PhaseTabsSelector);

        await tabs.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });
        await Expect(tabs).ToHaveCountAsync(2);

        return tabs;
    }
}
