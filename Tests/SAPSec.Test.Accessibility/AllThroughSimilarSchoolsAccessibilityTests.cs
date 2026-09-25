using FluentAssertions;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class AllThroughSimilarSchoolsAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    [Fact]
    public async Task PhaseTabs_AreKeyboardAccessibleAndStackOnMobile()
    {
        await NavigateTo(Routes.AllThroughSchool("100171").ViewSimilarSchools);

        var tabs = Page.Locator(".app-phase-tabs__tab");
        await tabs.Nth(0).FocusAsync();

        (await Page.EvaluateAsync<bool>("() => document.activeElement?.textContent?.trim() === 'Primary'"))
            .Should().BeTrue();

        await tabs.Nth(1).FocusAsync();

        (await Page.EvaluateAsync<bool>("() => document.activeElement?.textContent?.trim() === 'Secondary'"))
            .Should().BeTrue();

        await Expect(tabs.Nth(0)).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "false");

        await Page.SetViewportSizeAsync(375, 667);
        await NavigateTo(Routes.AllThroughSchool("100171").ViewSimilarSchools);

        var primaryBox = await tabs.Nth(0).BoundingBoxAsync();
        var secondaryBox = await tabs.Nth(1).BoundingBoxAsync();

        primaryBox.Should().NotBeNull();
        secondaryBox.Should().NotBeNull();
        secondaryBox!.Y.Should().BeGreaterThan(primaryBox!.Y + primaryBox.Height - 1);
    }
}
