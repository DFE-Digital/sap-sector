using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class ComparisonSchoolDetailsAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    [Fact]
    public async Task MapMarkers_SupportKeyboardPopupFlow()
    {
        await NavigateTo(Routes.PrimarySchool("100171").Comparison("150318").SchoolDetails);

        var mapDetails = Page.Locator("#comparison-map-details");
        await mapDetails.Locator("summary").ClickAsync();

        var isExpanded = await mapDetails.GetAttributeAsync("open");
        isExpanded.Should().NotBeNull("the map details panel should open before interacting with the map");

        await Page.EvaluateAsync("() => window.dispatchEvent(new Event('map:shown'))");

        var markers = Page.Locator("#map .leaflet-marker-icon[data-map-focusable='true']");
        await markers.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var markerCount = await markers.CountAsync();
        markerCount.Should().Be(2, "compare mode should render the current and similar school markers without clustering");

        var firstMarker = markers.First;
        await firstMarker.FocusAsync();

        (await firstMarker.GetAttributeAsync("role")).Should().Be("button");
        (await firstMarker.GetAttributeAsync("aria-haspopup")).Should().Be("dialog");

        await Page.Keyboard.PressAsync("Enter");

        var closeButton = Page.Locator(".leaflet-popup-close-button");
        await closeButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var expandedMarkers = Page.Locator("#map .leaflet-marker-icon[data-map-focusable='true'][aria-expanded='true']");
        (await expandedMarkers.CountAsync()).Should().Be(1, "keyboard activation should expand a single map marker");
        (await closeButton.GetAttributeAsync("aria-label")).Should().NotBeNullOrWhiteSpace();

        var closeButtonFocused = await closeButton.EvaluateAsync<bool>("el => el === document.activeElement");
        closeButtonFocused.Should().BeTrue("keyboard activation should move focus into the popup");
    }
}
