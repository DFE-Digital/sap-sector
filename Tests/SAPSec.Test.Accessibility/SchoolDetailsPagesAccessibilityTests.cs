using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using System.Text.RegularExpressions;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class SchoolDetailsPagesAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    private static readonly string[] SchoolDetailsPagePaths = [
        Routes.PrimarySchool("100171").SchoolDetails,
        Routes.PrimarySchool("100171").Comparison("150318").SchoolDetails,
        Routes.SecondarySchool("100052").SchoolDetails,
        Routes.SecondarySchool("100052").Comparison("141617").SchoolDetails
    ];

    [Theory]
    [MemberData(nameof(ComparisonPages))]
    public async Task MapMarkers_SupportKeyboardPopupFlow(string path)
    {
        await NavigateTo(path);

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

        // Wait for focus to move to the close button
        await Page.WaitForFunctionAsync("() => document.activeElement?.classList.contains('leaflet-popup-close-button')");

        var expandedMarkers = Page.Locator("#map .leaflet-marker-icon[data-map-focusable='true'][aria-expanded='true']");
        (await expandedMarkers.CountAsync()).Should().Be(1, "keyboard activation should expand a single map marker");
        (await closeButton.GetAttributeAsync("aria-label")).Should().NotBeNullOrWhiteSpace();

        var closeButtonFocused = await closeButton.EvaluateAsync<bool>("el => el === document.activeElement");
        closeButtonFocused.Should().BeTrue("keyboard activation should move focus into the popup");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task SchoolDetails_IsResponsive_OnMobile(string path)
    {
        await Page.SetViewportSizeAsync(375, 667); // iPhone SE size
        await NavigateTo(path);

        var summaryList = Page.Locator(".govuk-summary-list").First;
        var isVisible = await summaryList.IsVisibleAsync();

        isVisible.Should().BeTrue("Summary list should be visible on mobile");
    }

    [Theory]
    [MemberData(nameof(AllPages))]
    public async Task SchoolDetails_IsResponsive_OnTablet(string path)
    {
        await Page.SetViewportSizeAsync(768, 1024); // iPad size
        await NavigateTo(path);

        var heading = Page.Locator("h1");
        var isVisible = await heading.IsVisibleAsync();

        isVisible.Should().BeTrue("Heading should be visible on tablet");
    }

    [Theory]
    [MemberData(nameof(ComparisonPages))]
    public async Task SchoolDetails_ShowsCompareServiceNavigation(string path)
    {
        await NavigateTo(path);

        var nav = Page.Locator("div.govuk-service-navigation.compare-nav nav[aria-label='Compare sections']");
        var count = await nav.CountAsync();

        count.Should().Be(1, "Compare sections service navigation should be visible");
    }

    [Theory]
    [MemberData(nameof(ComparisonPages))]
    public async Task SchoolDetails_MapContainer_HasExpectedAttributes(string path)
    {
        await NavigateTo(path);

        await Page.WaitForSelectorAsync("#map", new() { State = WaitForSelectorState.Attached, Timeout = 15000 });

        var map = Page.Locator("#map");
        (await map.CountAsync()).Should().Be(1, "Map container should exist");

        var mapMode = await map.GetAttributeAsync("data-map-mode");
        mapMode.Should().Be("compare");

        (await map.GetAttributeAsync("data-fixed-zoom")).Should().Be("14");
        (await map.GetAttributeAsync("role")).Should().Be("region");
        (await map.GetAttributeAsync("aria-label")).Should().Be("Map of schools");

        var loading = map.Locator(".map-loading");
        (await loading.CountAsync()).Should().BeGreaterThanOrEqualTo(0);

        if (await loading.CountAsync() > 0)
        {
            (await loading.First.TextContentAsync()).Should().Contain("Loading map");
        }
    }

    [Theory]
    [MemberData(nameof(ComparisonPages))]
    public async Task SchoolDetails_MapLegend_HasBothMarkerIcons(string path)
    {
        await NavigateTo(path);

        var mainMarker = Page.Locator("img.school-marker-icon[src='/assets/images/marker-school-pink.svg']");
        var similarMarker = Page.Locator("img.school-marker-icon[src='/assets/images/marker-school.svg']");

        (await mainMarker.CountAsync()).Should().Be(1, "Main school marker icon (pink) should be present");
        (await similarMarker.CountAsync()).Should().Be(1, "Similar school marker icon (blue) should be present");
    }

    public static TheoryData<string> AllPages()
    {
        var data = new TheoryData<string>();
        foreach (var path in SchoolDetailsPagePaths)
        {
            data.Add(path);
        }

        return data;
    }

    public static TheoryData<string> ComparisonPages()
    {
        var data = new TheoryData<string>();
        foreach (var path in SchoolDetailsPagePaths)
        {
            if (Regex.IsMatch(path, Routes.PrimarySchool(@"\d{6}").Comparison(@"\d{6}").SchoolDetails)
                || Regex.IsMatch(path, Routes.SecondarySchool(@"\d{6}").Comparison(@"\d{6}").SchoolDetails))
                data.Add(path);
        }

        return data;
    }
}
