using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SimilarSchoolsPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string SimilarSchoolsPath = "/school/108088/view-similar-schools";

    [Fact]
    public async Task SimilarSchoolsPage_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(SimilarSchoolsPath);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task SimilarSchoolsPage_ShowsFilterForm()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var filterForm = Page.Locator("#similar-schools-filter-form");
        var count = await filterForm.CountAsync();

        count.Should().Be(1, "Filter form should be present");
    }

    [Fact]
    public async Task SimilarSchoolsPage_ShowsResultsList()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var resultsList = Page.Locator(".app-school-results");
        var count = await resultsList.CountAsync();

        count.Should().Be(1, "Results list should be present");
    }

    [Fact]
    public async Task ToggleButton_HasCorrectInitialText()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleText = Page.Locator(".toggle-text");
        var text = await toggleText.TextContentAsync();

        text.Should().Be("View on map", "Initial toggle text should be 'View on map'");
    }

    [Fact]
    public async Task ListView_IsVisible_Initially()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var listView = Page.Locator("#listView");
        var isVisible = await listView.IsVisibleAsync();

        isVisible.Should().BeTrue("List view should be visible initially");
    }

    [Fact]
    public async Task MapView_IsHidden_Initially()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mapView = Page.Locator("#mapView");
        var hasHiddenClass = await mapView.EvaluateAsync<bool>("el => el.classList.contains('govuk-!-display-none')");

        hasHiddenClass.Should().BeTrue("Map view should be hidden initially");
    }

    [Fact]
    public async Task ClickToggle_ShowsMapView()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");
        await toggleLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        var mapView = Page.Locator("#mapView");
        var isVisible = await mapView.IsVisibleAsync();

        isVisible.Should().BeTrue("Map view should be visible after clicking toggle");
    }

    [Fact]
    public async Task MapView_ContainsMapElement()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");
        await toggleLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        var map = Page.Locator("#map");
        var count = await map.CountAsync();

        count.Should().Be(1, "Map element should exist in map view");
    }

    [Fact]
    public async Task SchoolsData_JsonExists()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var schoolsData = Page.Locator("#schools-data");
        var count = await schoolsData.CountAsync();

        count.Should().Be(1, "Schools data JSON element should exist");
    }

    [Fact]
    public async Task SchoolsData_HasCorrectType()
    {
        await Page.GotoAsync(SimilarSchoolsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var schoolsData = Page.Locator("#schools-data");
        var type = await schoolsData.GetAttributeAsync("type");

        type.Should().Be("application/json", "Schools data should have correct type attribute");
    }
}
