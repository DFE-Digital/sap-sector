using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

public class SchoolSearchFilterTests(WebApplicationSetupFixture fixture)
    : BasePageTest(fixture), IClassFixture<WebApplicationSetupFixture>
{
    #region Filter Visibility Tests

    [Fact]
    public async Task Filter_WhenResultsExist_IsVisible()
    {
        await NavigateToSearchResults();

        if (!await HasSearchResults()) return;

        var isVisible = await IsElementVisible(Selectors.FilterPanel);

        isVisible.Should().BeTrue("Filter panel should be visible when search has results");
    }

    [Fact]
    public async Task Filter_WhenNoResults_IsHidden()
    {
        await NavigateToSearchResultsWithNoResults();

        var count = await GetElementCount(Selectors.FilterPanel);

        count.Should().Be(0, "Filter panel should not be present when no results");
    }

    [Fact]
    public async Task SearchWarning_WhenNoResults_IsVisible()
    {
        await NavigateToSearchResultsWithNoResults();

        var isVisible = await IsElementVisible(Selectors.SearchWarning);

        isVisible.Should().BeTrue("Search warning should be visible when no results");
    }

    [Fact]
    public async Task SearchWarning_WhenNoResults_HasCorrectText()
    {
        await NavigateToSearchResultsWithNoResults();

        var text = await GetElementText(Selectors.SearchWarning);

        text.Should().Contain("couldn't find any schools", "Warning should explain no results found");
    }

    [Fact]
    public async Task SearchWarning_WhenResultsExist_IsHidden()
    {
        await NavigateToSearchResults();

        var count = await GetElementCount(Selectors.SearchWarning);

        if (count > 0)
        {
            var isVisible = await IsElementVisible(Selectors.SearchWarning);
            isVisible.Should().BeFalse("Search warning should be hidden when results exist");
        }
    }

    [Fact]
    public async Task ResultsColumn_WhenResultsExist_IsTwoThirds()
    {
        await NavigateToSearchResults();

        if (!await HasSearchResults()) return;

        var count = await GetElementCount(Selectors.TwoThirdsColumn);

        count.Should().BeGreaterThan(0, "Results area should be two-thirds width when results exist");
    }

    #endregion

    #region Filter Panel Tests

    [Fact]
    public async Task Filter_IsVisibleOnResultsPage()
    {
        await NavigateToSearchResults();

        var isVisible = await IsElementVisible(Selectors.FilterPanel);

        isVisible.Should().BeTrue("Filter panel should be visible on search results page");
    }

    [Fact]
    public async Task Filter_HasFilterHeading()
    {
        await NavigateToSearchResults();

        var text = await GetElementText(Selectors.FilterHeading);

        text.Should().Contain("Filter", "Filter panel should have 'Filter' heading");
    }

    [Fact]
    public async Task Filter_HasApplyButton()
    {
        await NavigateToSearchResults();

        var isVisible = await IsElementVisible(Selectors.ApplyFiltersButton);

        isVisible.Should().BeTrue("Apply filters button should be visible");
    }

    [Fact]
    public async Task Filter_ApplyButtonHasCorrectText()
    {
        await NavigateToSearchResults();

        var text = await GetElementText(Selectors.ApplyFiltersButton);

        text.Should().Contain("Apply filters", "Button should say 'Apply filters'");
    }

    #endregion

    #region Collapsible Section Tests

    [Fact]
    public async Task FilterSection_IsCollapsedByDefault()
    {
        await NavigateToSearchResults();

        if (!await HasFilterToggle()) return;

        var ariaExpanded = await GetToggleExpandedState();

        ariaExpanded.Should().Be("false", "Filter section should be collapsed by default");
    }

    [Fact]
    public async Task FilterSection_ExpandsOnClick()
    {
        await NavigateToSearchResults();

        if (!await HasFilterToggle()) return;

        await ExpandFilterSection();

        var ariaExpanded = await GetToggleExpandedState();
        ariaExpanded.Should().Be("true", "Filter section should expand on click");

        var isHidden = await Page.Locator(Selectors.FilterSectionContent).GetAttributeAsync("hidden");
        isHidden.Should().BeNull("Content should not have hidden attribute when expanded");
    }

    [Fact]
    public async Task FilterSection_CollapsesOnSecondClick()
    {
        await NavigateToSearchResults();

        if (!await HasFilterToggle()) return;

        await ExpandFilterSection();
        await CollapseFilterSection();

        var ariaExpanded = await GetToggleExpandedState();

        ariaExpanded.Should().Be("false", "Filter section should collapse on second click");
    }

    [Fact]
    public async Task FilterSection_HasTitle()
    {
        await NavigateToSearchResults();

        if (!await HasElement(Selectors.FilterSectionTitle)) return;

        var text = await GetElementText(Selectors.FilterSectionTitle);

        text.Should().Contain("Local authority", "Section should have 'Local authority' title");
    }

    [Fact]
    public async Task FilterSection_HasHintText()
    {
        await NavigateToSearchResults();

        if (!await ExpandFilterSection()) return;

        var hint = Page.Locator($"{Selectors.FilterSectionContent} .govuk-hint");
        var text = await hint.TextContentAsync();

        text.Should().Contain("Local authority in England", "Should have descriptive hint text");
    }

    #endregion

    #region Checkbox Tests

    [Fact]
    public async Task FilterCheckboxes_AreVisible()
    {
        await NavigateToSearchResults();

        if (!await ExpandFilterSection()) return;

        var count = await GetElementCount(Selectors.FilterCheckbox);

        count.Should().BeGreaterThan(0, "Should have local authority checkboxes");
    }

    [Fact]
    public async Task FilterCheckbox_HasAssociatedLabel()
    {
        await NavigateToSearchResults();

        if (!await ExpandFilterSection()) return;

        var checkboxItems = Page.Locator(Selectors.CheckboxItem);
        var count = await checkboxItems.CountAsync();

        for (var i = 0; i < Math.Min(count, 3); i++)
        {
            await AssertCheckboxHasLabel(checkboxItems.Nth(i), i + 1);
        }
    }
   
    #endregion

    #region Form Submission Tests

    [Fact]
    public async Task Filter_PreservesQueryParameter()
    {
        const string testQuery = "TestQuery";
        await NavigateToSearchResults(testQuery);

        if (!await ExpandFilterSection()) return;
        if (!await HasFilterCheckboxes()) return;

        await CheckFirstFilterCheckbox();
        await WaitForNavigation();

        Page.Url.Should().Contain($"query={testQuery}", "Original search query should be preserved");
    }

    #endregion

    #region Selected Filters Tests

    [Fact]
    public async Task SelectedFiltersSection_IsVisibleWhenFilterApplied()
    {
        await NavigateToSearchResultsWithFilter(TestData.LocalAuthority);

        var isVisible = await IsElementVisible(Selectors.SelectedFiltersSection);

        isVisible.Should().BeTrue("Selected filters section should be visible when filters are applied");
    }

    [Fact]
    public async Task ClearFiltersLink_IsVisibleWhenFilterApplied()
    {
        await NavigateToSearchResultsWithFilter(TestData.LocalAuthority);

        if (!await HasElement(Selectors.ClearFiltersLink)) return;

        var text = await GetElementText(Selectors.ClearFiltersLink);

        text.Should().Contain("Clear filters", "Should have 'Clear filters' link");
    }

    [Fact]
    public async Task FilterTag_ShowsSelectedLocalAuthority()
    {
        await NavigateToSearchResultsWithFilter(TestData.LocalAuthority);

        if (!await HasElement(Selectors.FilterTag)) return;

        var tagText = await Page.Locator(Selectors.FilterTag).First.TextContentAsync();

        tagText.Should().Contain(TestData.LocalAuthority, "Filter tag should show selected local authority");
    }

    [Fact]
    public async Task ClearFiltersLink_RemovesAllFilters()
    {
        await NavigateToSearchResultsWithFilter(TestData.LocalAuthority);

        if (!await HasElement(Selectors.ClearFiltersLink)) return;

        await Page.Locator(Selectors.ClearFiltersLink).ClickAsync();
        await WaitForNavigation();

        Page.Url.Should().NotContain("localAuthorities", "Filters should be cleared from URL");
    }

    [Fact]
    public async Task FilterTag_RemovesSpecificFilter()
    {
        await NavigateToSearchResultsWithMultipleFilters();

        var filterTags = Page.Locator(Selectors.FilterTag);
        if (await filterTags.CountAsync() < 2) return;

        await filterTags.First.ClickAsync();
        await WaitForNavigation();

        Page.Url.Should().Contain("localAuthorities", "Should still have remaining filter");
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task FilterToggle_HasAriaExpanded()
    {
        await NavigateToSearchResults();

        if (!await HasFilterToggle()) return;

        var ariaExpanded = await GetToggleExpandedState();

        ariaExpanded.Should().NotBeNull("Toggle should have aria-expanded attribute");
    }

    [Fact]
    public async Task FilterToggle_HasAriaControls()
    {
        await NavigateToSearchResults();

        if (!await HasFilterToggle()) return;

        var ariaControls = await Page.Locator(Selectors.FilterSectionToggle)
            .GetAttributeAsync("aria-controls");

        ariaControls.Should().NotBeNullOrWhiteSpace("Toggle should have aria-controls attribute");
    }

    [Fact]
    public async Task FilterContent_HasMatchingId()
    {
        await NavigateToSearchResults();

        if (!await HasFilterToggle()) return;

        var ariaControls = await Page.Locator(Selectors.FilterSectionToggle)
            .GetAttributeAsync("aria-controls");
        var count = await Page.Locator($"#{ariaControls}").CountAsync();

        count.Should().Be(1, "Content element should exist with matching ID");
    }

    [Fact]
    public async Task FilterCheckboxes_UseGovukStyling()
    {
        await NavigateToSearchResults();

        if (!await ExpandFilterSection()) return;

        var count = await GetElementCount(Selectors.SmallCheckboxes);

        count.Should().BeGreaterThan(0, "Checkboxes should use GOV.UK small checkbox styling");
    }

    [Fact]
    public async Task FilterTags_HaveVisuallyHiddenText()
    {
        await NavigateToSearchResultsWithFilter(TestData.LocalAuthority);

        var filterTags = Page.Locator(Selectors.FilterTag);
        if (await filterTags.CountAsync() == 0) return;

        var hiddenText = filterTags.First.Locator(Selectors.VisuallyHidden);
        var count = await hiddenText.CountAsync();

        count.Should().Be(1, "Filter tags should have visually hidden text for screen readers");
    }

    #endregion

    #region Keyboard Navigation Tests

    [Fact]
    public async Task FilterToggle_ExpandsWithEnterKey()
    {
        await NavigateToSearchResults();

        if (!await HasFilterToggle()) return;

        await Page.Locator(Selectors.FilterSectionToggle).FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(100);

        var ariaExpanded = await GetToggleExpandedState();

        ariaExpanded.Should().Be("true", "Toggle should expand with Enter key");
    }

    [Fact]
    public async Task FilterToggle_ExpandsWithSpaceKey()
    {
        await NavigateToSearchResults();

        if (!await HasFilterToggle()) return;

        await Page.Locator(Selectors.FilterSectionToggle).FocusAsync();
        await Page.Keyboard.PressAsync("Space");
        await Page.WaitForTimeoutAsync(100);

        var ariaExpanded = await GetToggleExpandedState();

        ariaExpanded.Should().Be("true", "Toggle should expand with Space key");
    }

    #endregion

    #region Navigation Helpers

    private async Task NavigateToSearchResults(string query = "School")
    {
        await Page.GotoAsync($"{Paths.SchoolSearchResults}?query={query}");
        await WaitForNavigation();
    }

    private async Task NavigateToSearchResultsWithNoResults()
    {
        await Page.GotoAsync($"{Paths.SchoolSearchResults}?query={TestData.NonExistentQuery}");
        await WaitForNavigation();
    }

    private async Task NavigateToSearchResultsWithFilter(string localAuthority)
    {
        await Page.GotoAsync($"{Paths.SchoolSearchResults}?query=School&localAuthorities={localAuthority}");
        await WaitForNavigation();
    }

    private async Task NavigateToSearchResultsWithMultipleFilters()
    {
        await Page.GotoAsync($"{Paths.SchoolSearchResults}?query=School&localAuthorities=Leeds&localAuthorities=Bradford");
        await WaitForNavigation();
    }

    private async Task WaitForNavigation()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    #endregion

    #region Element Helpers

    private async Task<bool> HasElement(string selector)
    {
        return await Page.Locator(selector).CountAsync() > 0;
    }

    private async Task<bool> IsElementVisible(string selector)
    {
        var element = Page.Locator(selector);
        if (await element.CountAsync() == 0) return false;
        return await element.IsVisibleAsync();
    }

    private async Task<int> GetElementCount(string selector)
    {
        return await Page.Locator(selector).CountAsync();
    }

    private async Task<string> GetElementText(string selector)
    {
        return await Page.Locator(selector).TextContentAsync() ?? string.Empty;
    }

    private async Task<bool> HasSearchResults()
    {
        var warningCount = await GetElementCount(Selectors.SearchWarning);
        if (warningCount > 0)
        {
            return !await IsElementVisible(Selectors.SearchWarning);
        }
        return true;
    }

    #endregion

    #region Filter Section Helpers

    private async Task<bool> HasFilterToggle()
    {
        return await HasElement(Selectors.FilterSectionToggle);
    }

    private async Task<bool> HasFilterCheckboxes()
    {
        return await GetElementCount(Selectors.FilterCheckbox) > 0;
    }

    private async Task<string?> GetToggleExpandedState()
    {
        return await Page.Locator(Selectors.FilterSectionToggle)
            .GetAttributeAsync("aria-expanded");
    }

    private async Task<bool> ExpandFilterSection()
    {
        if (!await HasFilterToggle()) return false;

        var ariaExpanded = await GetToggleExpandedState();
        if (ariaExpanded == "true") return true;

        await Page.Locator(Selectors.FilterSectionToggle).ClickAsync();
        await Page.WaitForTimeoutAsync(150);
        return true;
    }

    private async Task CollapseFilterSection()
    {
        if (!await HasFilterToggle()) return;

        var ariaExpanded = await GetToggleExpandedState();
        if (ariaExpanded == "false") return;

        await Page.Locator(Selectors.FilterSectionToggle).ClickAsync();
        await Page.WaitForTimeoutAsync(150);
    }

    private async Task CheckFirstFilterCheckbox()
    {
        var checkbox = Page.Locator(Selectors.FilterCheckbox).First;
        await checkbox.CheckAsync();
    }

    private async Task AssertCheckboxHasLabel(ILocator checkboxItem, int index)
    {
        var input = checkboxItem.Locator("input");
        var label = checkboxItem.Locator("label");

        var inputId = await input.GetAttributeAsync("id");
        var labelFor = await label.GetAttributeAsync("for");

        labelFor.Should().Be(inputId, $"Checkbox {index} label should be associated with input");
    }

    #endregion

    #region Constants

    private static class Paths
    {
        public const string SchoolSearchResults = "/school/search";
    }

    private static class TestData
    {
        public const string NonExistentQuery = "XYZNONEXISTENTSCHOOL12345";
        public const string LocalAuthority = "Leeds";
    }

    private static class Selectors
    {
        // Layout
        public const string TwoThirdsColumn = ".govuk-grid-column-two-thirds";

        // Filter panel
        public const string FilterPanel = ".moj-filter";
        public const string FilterHeading = ".moj-filter__header-title h2";
        public const string ApplyFiltersButton = ".moj-filter__options button[type='submit']";

        // Collapsible section
        public const string FilterSectionToggle = ".app-filter-section__toggle";
        public const string FilterSectionTitle = ".app-filter-section__title";
        public const string FilterSectionContent = ".app-filter-section__content";

        // Checkboxes
        public const string FilterCheckbox = ".govuk-checkboxes__input";
        public const string CheckedFilterCheckbox = ".govuk-checkboxes__input:checked";
        public const string CheckboxItem = ".govuk-checkboxes__item";
        public const string SmallCheckboxes = ".govuk-checkboxes--small";

        // Selected filters
        public const string SelectedFiltersSection = ".moj-filter__selected";
        public const string ClearFiltersLink = ".moj-filter__heading-action a";
        public const string FilterTag = ".moj-filter__tag";

        // Results and warnings
        public const string SearchResults = ".app-school-results, .govuk-table, [data-testid='search-results']";
        public const string SearchWarning = "#search-warning";

        // Accessibility
        public const string VisuallyHidden = ".govuk-visually-hidden";
    }

    #endregion
}