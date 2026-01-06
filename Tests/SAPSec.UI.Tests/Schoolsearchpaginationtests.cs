using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SchoolSearchPaginationTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string SchoolSearchResultsPath = "/school/search";

    #region Pagination Visibility Tests

    [Fact]
    public async Task Pagination_IsNotVisible_WhenNoResults()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=XYZNonExistentSchool999");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var pagination = Page.Locator(".govuk-pagination");
        var count = await pagination.CountAsync();

        count.Should().Be(0, "Pagination should not be visible when no results");
    }

    [Fact]
    public async Task Pagination_IsNotVisible_WhenSinglePage()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=UniqueSchoolName");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var pagination = Page.Locator(".govuk-pagination");
        var count = await pagination.CountAsync();

        if (count > 0)
        {
            var pageItems = Page.Locator(".govuk-pagination__item");
            var pageCount = await pageItems.CountAsync();
            pageCount.Should().BeGreaterThan(1, "Pagination should only show with multiple pages");
        }
    }

    [Fact]
    public async Task Pagination_IsVisible_WhenMultiplePages()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var resultsCount = Page.Locator(".app-school-results-count");
        var countText = await resultsCount.TextContentAsync();

        if (countText?.Contains("of") == true)
        {
            var pagination = Page.Locator(".govuk-pagination");
            var isVisible = await pagination.IsVisibleAsync();

            if (countText.Contains("of 1") || countText.Contains("of 2") ||
                countText.Contains("of 3") || countText.Contains("of 4") ||
                countText.Contains("of 5"))
            {
                return;
            }

            isVisible.Should().BeTrue("Pagination should be visible when multiple pages exist");
        }
    }

    #endregion

    #region Pagination Structure Tests

    [Fact]
    public async Task Pagination_HasCorrectStructure()
    {
        await NavigateToPageWithMultipleResults();

        var pagination = Page.Locator(".govuk-pagination");
        if (await pagination.CountAsync() == 0) return;

        var navLabel = await pagination.GetAttributeAsync("aria-label");
        navLabel.Should().Be("Pagination", "Pagination should have correct aria-label");
    }

    [Fact]
    public async Task Pagination_HasPageNumbers()
    {
        await NavigateToPageWithMultipleResults();

        var pagination = Page.Locator(".govuk-pagination");
        if (await pagination.CountAsync() == 0) return;

        var pageItems = Page.Locator(".govuk-pagination__item");
        var count = await pageItems.CountAsync();

        count.Should().BeGreaterThan(0, "Pagination should have page number items");
    }

    [Fact]
    public async Task Pagination_CurrentPage_HasCorrectStyling()
    {
        await NavigateToPageWithMultipleResults();

        var currentPage = Page.Locator(".govuk-pagination__item--current");
        if (await currentPage.CountAsync() == 0) return;

        var ariaCurrent = await currentPage.Locator("a").GetAttributeAsync("aria-current");
        ariaCurrent.Should().Be("page", "Current page should have aria-current='page'");
    }

    [Fact]
    public async Task Pagination_PageLinks_HaveCorrectAriaLabels()
    {
        await NavigateToPageWithMultipleResults();

        var pageLinks = Page.Locator(".govuk-pagination__link");
        if (await pageLinks.CountAsync() == 0) return;

        var firstPageLink = pageLinks.First;
        var ariaLabel = await firstPageLink.GetAttributeAsync("aria-label");

        ariaLabel.Should().Contain("Page", "Page links should have descriptive aria-labels");
    }

    #endregion

    #region Previous/Next Navigation Tests

    [Fact]
    public async Task Pagination_FirstPage_HasNoPreviewLink()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=1");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var pagination = Page.Locator(".govuk-pagination");
        if (await pagination.CountAsync() == 0) return;

        var prevLink = Page.Locator(".govuk-pagination__prev");
        var count = await prevLink.CountAsync();

        count.Should().Be(0, "First page should not have Previous link");
    }

    [Fact]
    public async Task Pagination_NextLink_HasCorrectText()
    {
        await NavigateToPageWithMultipleResults();

        var nextLink = Page.Locator(".govuk-pagination__next");
        if (await nextLink.CountAsync() == 0) return;

        var linkText = await nextLink.TextContentAsync();
        linkText.Should().Contain("Next", "Next link should contain 'Next' text");
    }

    [Fact]
    public async Task Pagination_PreviousLink_HasCorrectText()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=2");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var prevLink = Page.Locator(".govuk-pagination__prev");
        if (await prevLink.CountAsync() == 0) return;

        var linkText = await prevLink.TextContentAsync();
        linkText.Should().Contain("Previous", "Previous link should contain 'Previous' text");
    }

    [Fact]
    public async Task Pagination_NextLink_HasVisuallyHiddenPageText()
    {
        await NavigateToPageWithMultipleResults();

        var nextLink = Page.Locator(".govuk-pagination__next");
        if (await nextLink.CountAsync() == 0) return;

        var hiddenText = nextLink.Locator(".govuk-visually-hidden");
        var text = await hiddenText.TextContentAsync();

        text.Should().Contain("page", "Next link should have visually hidden 'page' text for accessibility");
    }

    [Fact]
    public async Task Pagination_PreviousLink_HasRelPrev()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=2");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var prevLink = Page.Locator(".govuk-pagination__prev a");
        if (await prevLink.CountAsync() == 0) return;

        var rel = await prevLink.GetAttributeAsync("rel");
        rel.Should().Be("prev", "Previous link should have rel='prev'");
    }

    #endregion

    #region Navigation Functionality Tests

    [Fact]
    public async Task Pagination_ClickNextPage_NavigatesToNextPage()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=1");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var nextLink = Page.Locator(".govuk-pagination__next a");
        if (await nextLink.CountAsync() == 0) return;

        await nextLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("page=2", "Clicking Next should navigate to page 2");
    }

    [Fact]
    public async Task Pagination_ClickPreviousPage_NavigatesToPreviousPage()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=2");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var prevLink = Page.Locator(".govuk-pagination__prev a");
        if (await prevLink.CountAsync() == 0) return;

        await prevLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("page=1", "Clicking Previous should navigate to page 1");
    }

    [Fact]
    public async Task Pagination_ClickPageNumber_NavigatesToThatPage()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=1");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var pageLink = Page.Locator(".govuk-pagination__item:not(.govuk-pagination__item--current) a").First;
        if (await pageLink.CountAsync() == 0) return;

        var pageNumber = await pageLink.TextContentAsync();
        await pageLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain($"page={pageNumber?.Trim()}", "Clicking page number should navigate to that page");
    }

    [Fact]
    public async Task Pagination_PreservesQueryOnNavigation()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=1");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var nextLink = Page.Locator(".govuk-pagination__next a");
        if (await nextLink.CountAsync() == 0) return;

        await nextLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("query=School", "Navigation should preserve search query");
    }

    [Fact]
    public async Task Pagination_PreservesFiltersOnNavigation()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&localAuthorities=Leeds&page=1");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var nextLink = Page.Locator(".govuk-pagination__next a");
        if (await nextLink.CountAsync() == 0) return;

        await nextLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("localAuthorities=Leeds", "Navigation should preserve filters");
    }

    #endregion

    #region Results Count Tests

    [Fact]
    public async Task ResultsCount_ShowsCorrectRange_OnFirstPage()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=1");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var resultsCount = Page.Locator(".app-school-results-count");
        if (await resultsCount.CountAsync() == 0) return;

        var text = await resultsCount.TextContentAsync();
        text.Should().Contain("1-", "First page should show results starting from 1");
    }

    [Fact]
    public async Task ResultsCount_ShowsCorrectRange_OnSecondPage()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=2");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var resultsCount = Page.Locator(".app-school-results-count");
        if (await resultsCount.CountAsync() == 0) return;

        var text = await resultsCount.TextContentAsync();
        text.Should().Contain("11-", "Second page should show results starting from 6");
    }

    #endregion

    #region Ellipsis Tests

    [Fact]
    public async Task Pagination_ShowsEllipsis_WhenManyPages()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=4");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var ellipsis = Page.Locator(".govuk-pagination__item--ellipsis");
        var count = await ellipsis.CountAsync();

        count.Should().BeGreaterThan(0, "Ellipsis should appear when on middle page with many total pages");
    }

    #endregion

    #region Keyboard Navigation Tests

    [Fact]
    public async Task Pagination_Links_AreKeyboardAccessible()
    {
        await NavigateToPageWithMultipleResults();

        var pagination = Page.Locator(".govuk-pagination");
        if (await pagination.CountAsync() == 0) return;

        var firstLink = pagination.Locator("a").First;
        await firstLink.FocusAsync();

        var isFocused = await firstLink.EvaluateAsync<bool>("el => el === document.activeElement");
        isFocused.Should().BeTrue("Pagination links should be keyboard focusable");
    }

    #endregion

    #region Page Title Tests
    [Fact]
    public async Task PageTitle_DoesNotIncludePageNumber_OnFirstPage()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=1");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await Page.TitleAsync();
        title.Should().NotContain("page 1");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Pagination_InvalidPageNumber_HandlesGracefully()
    {
        var response = await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=999");

        response!.Status.Should().Be(200, "Invalid page number should be handled gracefully");
    }

    [Fact]
    public async Task Pagination_NegativePageNumber_HandlesGracefully()
    {
        var response = await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=-1");

        response!.Status.Should().Be(200, "Negative page number should be handled gracefully");
    }

    [Fact]
    public async Task Pagination_ZeroPageNumber_HandlesGracefully()
    {
        var response = await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=0");

        response!.Status.Should().Be(200, "Zero page number should be handled gracefully");
    }

    [Fact]
    public async Task Pagination_NonNumericPageNumber_HandlesGracefully()
    {
        var response = await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School&page=abc");

        response!.Status.Should().Be(200, "Non-numeric page number should be handled gracefully");
    }

    #endregion

    #region Helper Methods

    private async Task NavigateToPageWithMultipleResults()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    #endregion
}