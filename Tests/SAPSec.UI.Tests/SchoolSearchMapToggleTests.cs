using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SchoolSearchMapToggleTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string SchoolSearchResultsPath = "/find-a-school/search";

    #region Toggle Button Tests

    [Fact]
    public async Task ToggleButton_IsVisible_WhenResultsExist()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleWrap = Page.Locator("#toggleWrap");
        var toggleLink = Page.Locator("#toggleViewLink");

        var wrapIsVisible = await toggleWrap.IsVisibleAsync();
        var linkIsVisible = await toggleLink.IsVisibleAsync();

        wrapIsVisible.Should().BeTrue("Toggle wrapper should be visible when results exist");
        linkIsVisible.Should().BeTrue("Toggle link should be visible when results exist");
    }

    [Fact]
    public async Task ToggleButton_IsNotVisible_WhenNoResults()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=XYZNonExistentSchool999");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleWrap = Page.Locator("#toggleWrap");
        var count = await toggleWrap.CountAsync();

        count.Should().Be(0, "Toggle button should not be visible when no results");
    }

    [Fact]
    public async Task ToggleButton_HasCorrectInitialText()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleText = Page.Locator(".toggle-text");
        var text = await toggleText.TextContentAsync();

        text.Should().Be("View on map", "Initial toggle text should be 'View on map'");
    }

    [Fact]
    public async Task ToggleButton_HasCorrectDataAttribute()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");
        var dataView = await toggleLink.GetAttributeAsync("data-view");

        dataView.Should().Be("list", "Initial data-view attribute should be 'list'");
    }

    [Fact]
    public async Task ToggleButton_HasMapIcon_Initially()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mapIcon = Page.Locator(".toggle-icon--map");
        var isVisible = await mapIcon.IsVisibleAsync();

        isVisible.Should().BeTrue("Map icon should be visible initially");
    }

    [Fact]
    public async Task ToggleButton_HasListIcon_Initially()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var listIcon = Page.Locator(".toggle-icon--list");
        var count = await listIcon.CountAsync();

        count.Should().BeGreaterThan(0, "List icon should exist in DOM");
    }

    #endregion

    #region View Switching Tests

    [Fact]
    public async Task ListView_IsVisible_Initially()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var listView = Page.Locator("#listView");
        var isVisible = await listView.IsVisibleAsync();

        isVisible.Should().BeTrue("List view should be visible initially");
    }

    [Fact]
    public async Task MapView_IsHidden_Initially()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mapView = Page.Locator("#mapView");
        var hasHiddenClass = await mapView.EvaluateAsync<bool>("el => el.classList.contains('govuk-!-display-none')");

        hasHiddenClass.Should().BeTrue("Map view should be hidden initially");
    }

    [Fact]
    public async Task ClickToggle_ShowsMapView()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");
        await toggleLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for JS to execute

        var mapView = Page.Locator("#mapView");
        var isVisible = await mapView.IsVisibleAsync();

        isVisible.Should().BeTrue("Map view should be visible after clicking toggle");
    }

    [Fact]
    public async Task ClickToggle_HidesListView()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");
        await toggleLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for JS to execute

        var listView = Page.Locator("#listView");
        var hasHiddenClass = await listView.EvaluateAsync<bool>("el => el.classList.contains('govuk-!-display-none')");

        hasHiddenClass.Should().BeTrue("List view should be hidden after clicking toggle");
    }

    [Fact]
    public async Task ClickToggleTwice_ReturnsToListView()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");

        // Click to map view
        await toggleLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Click back to list view
        await toggleLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        var listView = Page.Locator("#listView");
        var isVisible = await listView.IsVisibleAsync();

        isVisible.Should().BeTrue("List view should be visible after toggling twice");
    }

    [Fact]
    public async Task ToggleButton_ChangesText_WhenSwitchingToMap()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");
        await toggleLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        var toggleText = Page.Locator(".toggle-text");
        var text = await toggleText.TextContentAsync();

        text.Should().Be("View as a list", "Toggle text should change to 'View as a list' when showing map");
    }

    #endregion

    #region Map View Tests

    [Fact]
    public async Task MapView_ContainsMapElement()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var map = Page.Locator("#map");
        var count = await map.CountAsync();

        count.Should().Be(1, "Map element should exist in map view");
    }

    [Fact]
    public async Task MapView_HasAriaLabel()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var map = Page.Locator("#map");
        var ariaLabel = await map.GetAttributeAsync("aria-label");

        ariaLabel.Should().Be("Map of schools", "Map should have descriptive aria-label");
    }

    [Fact]
    public async Task MapView_HasRoleRegion()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var map = Page.Locator("#map");
        var role = await map.GetAttributeAsync("role");

        role.Should().Be("region", "Map should have role='region' for accessibility");
    }

    [Fact]
    public async Task MapView_HasFixedZoomAttribute()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var map = Page.Locator("#map");
        var zoom = await map.GetAttributeAsync("data-fixed-zoom");

        zoom.Should().Be("14", "Map should have fixed zoom level");
    }

    #endregion

    #region Map Count Tests

    [Fact]
    public async Task MapCount_DisplaysCorrectly()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mapCount = Page.Locator("#mapCount");
        var count = await mapCount.CountAsync();

        count.Should().Be(1, "Map count element should exist");
    }

    [Fact]
    public async Task MapCount_ShowsTotalSchools()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mapCount = Page.Locator("#mapCount");
        var text = await mapCount.TextContentAsync();

        text.Should().Contain("schools", "Map count should contain 'schools' text");
        text.Should().Contain("Showing", "Map count should contain 'Showing' text");
    }

    [Fact]
    public async Task MapCount_HasCorrectCssClass()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mapCount = Page.Locator("#mapCount");
        var hasClass = await mapCount.EvaluateAsync<bool>("el => el.classList.contains('app-school-results-count')");

        hasClass.Should().BeTrue("Map count should have 'app-school-results-count' class");
    }

    #endregion

    #region Schools Data Tests

    [Fact]
    public async Task SchoolsData_JsonExists()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var schoolsData = Page.Locator("#schools-data");
        var count = await schoolsData.CountAsync();

        count.Should().Be(1, "Schools data JSON element should exist");
    }

    [Fact]
    public async Task SchoolsData_HasCorrectType()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var schoolsData = Page.Locator("#schools-data");
        var type = await schoolsData.GetAttributeAsync("type");

        type.Should().Be("application/json", "Schools data should have correct type attribute");
    }

    [Fact]
    public async Task SchoolsData_ContainsValidJson()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var schoolsData = Page.Locator("#schools-data");
        var jsonContent = await schoolsData.TextContentAsync();

        jsonContent.Should().NotBeNullOrWhiteSpace("Schools data should contain JSON");
        jsonContent?.Trim().Should().StartWith("[", "Schools data should be a JSON array");
    }

    #endregion

    #region Map Bar Actions Tests

    [Fact]
    public async Task MapBarActions_ElementExists()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mapBarActions = Page.Locator("#mapBarActions");
        var count = await mapBarActions.CountAsync();

        count.Should().Be(1, "Map bar actions element should exist");
    }

    #endregion

    #region Toggle Link Accessibility Tests

    [Fact]
    public async Task ToggleLink_IsKeyboardAccessible()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");
        await toggleLink.FocusAsync();

        var isFocused = await toggleLink.EvaluateAsync<bool>("el => el === document.activeElement");
        isFocused.Should().BeTrue("Toggle link should be keyboard focusable");
    }

    [Fact]
    public async Task ToggleLink_HasNoVisitedState()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");
        var hasClass = await toggleLink.EvaluateAsync<bool>("el => el.classList.contains('govuk-link--no-visited-state')");

        hasClass.Should().BeTrue("Toggle link should have no-visited-state class");
    }

    [Fact]
    public async Task ToggleIcons_HaveAriaHidden()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mapIcon = Page.Locator(".toggle-icon--map");
        var ariaHidden = await mapIcon.GetAttributeAsync("aria-hidden");

        ariaHidden.Should().Be("true", "Toggle icons should be aria-hidden");
    }

    #endregion
}
