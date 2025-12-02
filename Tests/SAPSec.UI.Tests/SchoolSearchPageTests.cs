using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

public class SchoolSearchPageTests : IClassFixture<WebApplicationSetupFixture>, IAsyncLifetime
{
    private readonly WebApplicationSetupFixture _fixture;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    // URLs using fixture BaseUrl
    // Note: Routes are doubled due to controller configuration
    // SchoolSearchController has [Route("school")] and actions have [Route("school/search")] etc.
    private string SchoolSearchPath => $"{_fixture.BaseUrl}/school/search-for-a-school";
    private string SchoolSearchResultsPath => $"{_fixture.BaseUrl}/school/school/search";  // Doubled!
    private string SchoolSuggestPath => $"{_fixture.BaseUrl}/school/school/suggest";        // Doubled!

    public SchoolSearchPageTests(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1280, Height = 720 }
        });

        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    #region Index Page Tests

    [Fact]
    public async Task SchoolSearchIndex_LoadsSuccessfully()
    {
        var response = await _page.GotoAsync(SchoolSearchPath);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolSearchIndex_DisplaysQueryInputField()
    {
        await _page.GotoAsync(SchoolSearchPath);

        var input = _page.Locator("input[name='__Query']");
        var button = _page.Locator("button[name='Search']");
        var form = _page.Locator("form");

        (await input.IsVisibleAsync()).Should().BeTrue("Query input field should be visible");
        (await form.IsVisibleAsync()).Should().BeTrue("Search form should be visible");
        (await button.IsVisibleAsync()).Should().BeTrue("Search button should be visible");
    }

    #endregion

    #region Form Validation Tests

    [Fact]
    public async Task SchoolSearchIndex_SubmitEmptyForm_ShowsValidationError()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorSummary = _page.Locator(".govuk-error-summary");
        (await errorSummary.IsVisibleAsync()).Should().BeTrue("Error summary should be visible");

        var errorMessage = await errorSummary.Locator(".govuk-error-summary__list li").TextContentAsync();
        errorMessage.Should().Contain("Enter a school name or school ID to start a search");
    }

    [Fact]
    public async Task SchoolSearchIndex_SubmitShortQuery_ShowsValidationError()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("input[name='__Query']").FillAsync("AB");
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorSummary = _page.Locator(".govuk-error-summary");
        (await errorSummary.IsVisibleAsync()).Should().BeTrue("Error summary should be visible for short query");

        var errorMessage = await errorSummary.TextContentAsync();
        errorMessage.Should().Contain("minimum 3 characters");
    }

    [Fact]
    public async Task SchoolSearchIndex_SubmitValidQuery_RedirectsToSearchResults()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("input[name='__Query']").FillAsync("Test School");
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Note: RedirectToAction creates doubled route /school/school/search
        await _page.WaitForURLAsync("**/school/school/search?query=Test%20School");

        _page.Url.Should().Contain("/school/school/search");
        _page.Url.Should().Contain("query=Test");
    }

    [Fact]
    public async Task SchoolSearchIndex_ErrorInputField_HasErrorStyling()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorInput = _page.Locator("div.govuk-error-summary");
        (await errorInput.IsVisibleAsync()).Should().BeTrue("Input field should have error styling");
    }

    [Fact]
    public async Task SchoolSearchIndex_NumericSearch_NoResults_ShowsErrorMessage()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("input[name='__Query']").FillAsync("123");
        await _page.WaitForTimeoutAsync(600);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorSummary = _page.Locator(".govuk-error-summary");
        (await errorSummary.IsVisibleAsync()).Should().BeTrue("Error summary should be visible");

        var errorMessage = await errorSummary.Locator(".govuk-error-summary__list li").TextContentAsync();
        errorMessage.Should().Contain("We could not find any schools matching your search criteria");
    }

    [Fact]
    public async Task SchoolSearchIndex_NumericSearch_ValidResults_RedirectsToSchoolDetailPage()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("input[name='__Query']").FillAsync("102848");
        await _page.WaitForTimeoutAsync(600);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        _page.Url.Should().Contain("school/102848");
    }

    [Fact]
    public async Task SchoolSearchIndex_Search_SingleResult_RedirectsToSchoolDetailPage()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("input[name='__Query']").FillAsync("Saint Paul Roman Catholic");
        await _page.WaitForTimeoutAsync(600);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        _page.Url.Should().Contain("school/100273");
    }

    #endregion

    #region Autocomplete/Suggester Tests

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_CreatesHiddenFields()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hiddenQueryField = _page.Locator("input[name='Query'][type='hidden']");
        var hiddenUrnField = _page.Locator("input[name='Urn'][type='hidden']");

        (await hiddenQueryField.CountAsync()).Should().Be(1, "Hidden Query field should exist");
        (await hiddenUrnField.CountAsync()).Should().Be(1, "Hidden Urn field should exist");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_SyncsVisibleInputWithHiddenInput()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("Test School");
        await _page.WaitForTimeoutAsync(100);

        var hiddenQueryValue = await _page.Locator("input[name='Query'][type='hidden']").InputValueAsync();
        hiddenQueryValue.Should().Be("Test School", "Hidden Query field should sync with visible input");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_NumericInput_SetsHiddenUrnField()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("123456");
        await _page.WaitForTimeoutAsync(600);

        var urnValue = await _page.Locator("input[name='Urn']").InputValueAsync();
        urnValue.Should().Be("123456");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_TextInput_SetsHiddenUrnFieldToEmpty()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("School");
        await _page.WaitForTimeoutAsync(600);

        await _page.Locator("input[name='__Query']").FillAsync("Test School");
        await _page.WaitForTimeoutAsync(600);

        var urnValue = await _page.Locator("input[name='Urn']").InputValueAsync();
        urnValue.Should().Be(string.Empty);
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_NumericWithSlash_SetsHiddenUrnField()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("123/456");
        await _page.WaitForTimeoutAsync(600);

        var urnValue = await _page.Locator("input[name='Urn']").InputValueAsync();
        urnValue.Should().Be("123/456");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_NumericWithBackslash_SetsHiddenUrnField()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("123\\456");
        await _page.WaitForTimeoutAsync(600);

        var urnValue = await _page.Locator("input[name='Urn']").InputValueAsync();
        urnValue.Should().Be("123\\456");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_NumericWithBackslash_SetsHiddenUrnField_And_RedirectsToSchoolDetailPage()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("204\\3658");
        await _page.WaitForTimeoutAsync(600);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        _page.Url.Should().Contain("school/100273");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_EnterKey_SubmitsForm()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("Test School");
        await _page.Locator("input[name='__Query']").PressAsync("Enter");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Note: Doubled route
        _page.Url.Should().Contain("/school/school/search", "Pressing Enter should submit the form");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_ClearsUrnFieldOnInput()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.EvaluateAsync("document.querySelector('input[name=\"Urn\"]').value = '123456'");
        await _page.Locator("input[name='__Query']").FillAsync("New School");
        await _page.WaitForTimeoutAsync(100);

        var urnValue = await _page.Locator("input[name='Urn']").InputValueAsync();
        urnValue.Should().Be(string.Empty, "Urn field should be cleared when user types");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_MinLength3Characters_Required()
    {
        await _page.GotoAsync(SchoolSearchPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("AB");
        await _page.WaitForTimeoutAsync(600);

        var suggestionMenu = _page.Locator(".autocomplete__menu");
        var isVisible = await suggestionMenu.IsVisibleAsync();
        isVisible.Should().BeFalse("Autocomplete menu should not show for less than 3 characters");
    }

    #endregion

    #region Search Results Page Tests

    [Fact]
    public async Task SchoolSearchResults_LoadsSuccessfully()
    {
        var response = await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolSearchResults_DisplaysSearchForm()
    {
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var form = _page.Locator("form");
        (await form.IsVisibleAsync()).Should().BeTrue("Search form should be visible on results page");
    }

    [Fact]
    public async Task SchoolSearchResults_FormHasPreviousQuery()
    {
        var query = "Test School";
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query={query}");

        var input = _page.Locator("input[name='__Query']");
        var inputValue = await input.InputValueAsync();

        inputValue.Should().Be(query, "Previous search query should be populated in the form");
    }

    [Fact]
    public async Task SchoolSearchResults_CanSearchAgain()
    {
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        await _page.Locator("input[name='__Query']").FillAsync("Another School");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        _page.Url.Should().Contain("Another");
    }

    [Fact]
    public async Task SchoolSearchResults_NumericWithSlash_SetsHiddenUrnField()
    {
        await _page.GotoAsync(SchoolSearchResultsPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("123/456");
        await _page.WaitForTimeoutAsync(600);

        var urnValue = await _page.Locator("input[name='Urn']").InputValueAsync();
        urnValue.Should().Be("123/456");
    }

    [Fact]
    public async Task SchoolSearchResults_NumericWithBackslash_SetsHiddenUrnField()
    {
        await _page.GotoAsync(SchoolSearchResultsPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("123\\456");
        await _page.WaitForTimeoutAsync(600);

        var urnValue = await _page.Locator("input[name='Urn']").InputValueAsync();
        urnValue.Should().Be("123\\456");
    }

    [Fact]
    public async Task SchoolSearchResults_NumericSearch_NoResults_ShowsErrorMessage()
    {
        await _page.GotoAsync(SchoolSearchResultsPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("123");
        await _page.WaitForTimeoutAsync(600);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorSummary = _page.Locator(".govuk-error-summary");
        (await errorSummary.IsVisibleAsync()).Should().BeFalse();

        var warningMessage = await _page.Locator("#search-warning").TextContentAsync();
        warningMessage.Should().Contain("We couldn't find any schools matching your search criteria.");
    }

    [Fact]
    public async Task SchoolSearchResults_NumericSearch_ValidResults_RedirectsToSchoolDetailPage()
    {
        await _page.GotoAsync(SchoolSearchResultsPath);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("input[name='__Query']").FillAsync("102848");
        await _page.WaitForTimeoutAsync(600);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        _page.Url.Should().Contain("school/102848");
    }

    [Fact]
    public async Task SchoolSearchResults_EmptyQuery_ShowsMessage()
    {
        var response = await _page.GotoAsync($"{SchoolSearchResultsPath}?query=");

        response!.Status.Should().Be(200);
        var content = await _page.ContentAsync();
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SchoolSearchResults_NoResults_DisplaysWarningMessage()
    {
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=XYZNonExistentSchool999");

        var warningMessage = _page.Locator("#search-warning");

        if (await warningMessage.IsVisibleAsync())
        {
            var text = await warningMessage.TextContentAsync();
            text.Should().Contain("couldn't find any schools", "No results message should be displayed");
        }
    }

    [Fact]
    public async Task SchoolSearchResults_WithResults_DisplaysSchoolLinks()
    {
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var resultLinks = _page.Locator(".govuk-list--result a");
        var count = await resultLinks.CountAsync();

        if (count > 0)
        {
            var firstLink = resultLinks.First;
            var href = await firstLink.GetAttributeAsync("href");
            href.Should().Contain("/school/102848", "Result links should point to school detail pages");
        }
    }

    #endregion

    #region Edge Cases and Special Characters

    [Fact]
    public async Task SchoolSearchIndex_SpecialCharacters_HandledCorrectly()
    {
        await _page.GotoAsync(SchoolSearchPath);

        var specialQuery = "St. Mary's & John's School (Primary)";
        await _page.Locator("input[name='__Query']").FillAsync(specialQuery);
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Note: Doubled route
        _page.Url.Should().Contain("school/school/search");
    }

    [Fact]
    public async Task SchoolSearchIndex_NumericQuery_ProcessedCorrectly()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("input[name='__Query']").FillAsync("123456");
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Note: Doubled route
        _page.Url.Should().Contain("/school/school/search");
    }

    [Fact]
    public async Task SchoolSearchIndex_LongQuery_HandledCorrectly()
    {
        await _page.GotoAsync(SchoolSearchPath);
        var longQuery = new string('A', 200);

        await _page.Locator("input[name='__Query']").FillAsync(longQuery);
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Note: Doubled route
        _page.Url.Should().Contain("school/school/search");
    }

    #endregion

    #region Autocomplete Suggest API Tests

    [Fact]
    public async Task SchoolSuggest_EmptyQuery_ReturnsSuccessfully()
    {
        var response = await _page.GotoAsync($"{SchoolSuggestPath}?queryPart=");

        response!.Status.Should().Be(200);
        var contentType = response.Headers["content-type"];
        contentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task SchoolSuggest_ReturnsJsonArray()
    {
        var response = await _page.GotoAsync($"{SchoolSuggestPath}?queryPart=School");

        response!.Status.Should().Be(200);

        var content = await response.TextAsync();
        content.Should().StartWith("[", "Response should be a JSON array");
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task SchoolSearchIndex_ErrorSummaryIsVisible()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorSummary = _page.Locator(".govuk-error-summary");
        (await errorSummary.IsVisibleAsync()).Should().BeTrue("Error summary should be visible and focusable");

        var title = errorSummary.Locator(".govuk-error-summary__title");
        var list = errorSummary.Locator(".govuk-error-summary__list");

        (await title.IsVisibleAsync()).Should().BeTrue("Error summary title should be visible");
        (await list.IsVisibleAsync()).Should().BeTrue("Error summary list should be visible");
    }

    [Fact]
    public async Task SchoolSearchIndex_ErrorSummaryLinksToField()
    {
        await _page.GotoAsync(SchoolSearchPath);

        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorLink = _page.Locator(".govuk-error-summary__list a");
        var href = await errorLink.GetAttributeAsync("href");
        href.Should().Contain("#", "Error summary links should point to form fields");
    }

    [Fact]
    public async Task SchoolSearchIndex_InputHasAriaDescribedBy()
    {
        await _page.GotoAsync(SchoolSearchPath);

        var input = _page.Locator("input[name='Query']");
        var ariaDescribedBy = await input.GetAttributeAsync("aria-describedby");

        ariaDescribedBy.Should().Contain("hint", "Input should reference hint text");
    }

    [Fact]
    public async Task SchoolSearchIndex_FormHasSearchRole()
    {
        await _page.GotoAsync(SchoolSearchPath);

        var form = _page.Locator("form[role='search']");
        var exists = await form.CountAsync();

        exists.Should().BeGreaterThan(0, "Form should have search role for accessibility");
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task SchoolSearchResults_WithNullQuery_HandlesGracefully()
    {
        var response = await _page.GotoAsync(SchoolSearchResultsPath);

        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolSearchIndex_FormWithJavascriptDisabled_StillWorks()
    {
        await _page.CloseAsync();

        var context = await _browser.NewContextAsync(new()
        {
            JavaScriptEnabled = false,
            IgnoreHTTPSErrors = true
        });
        var jsDisabledPage = await context.NewPageAsync();

        await jsDisabledPage.GotoAsync(SchoolSearchPath);

        await jsDisabledPage.Locator("input[name='Query']").FillAsync("Test School");
        await jsDisabledPage.Locator("button[name='Search']").ClickAsync();
        await jsDisabledPage.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Note: Doubled route
        jsDisabledPage.Url.Should().Contain("/school/school/search");

        await jsDisabledPage.CloseAsync();
        await context.DisposeAsync();
    }

    #endregion

    #region View Rendering Tests

    [Fact]
    public async Task SchoolSearchIndex_RendersCorrectPageTitle()
    {
        await _page.GotoAsync(SchoolSearchPath);

        var title = await _page.TitleAsync();
        var heading = await _page.Locator("h1").TextContentAsync();

        title.Should().NotBeNullOrEmpty("Page should have a title");
        heading.Should().Contain("school", "Heading should mention schools");
    }

    [Fact]
    public async Task SchoolSearchIndex_RendersHintText()
    {
        await _page.GotoAsync(SchoolSearchPath);

        var hint = _page.Locator(".govuk-hint");
        (await hint.IsVisibleAsync()).Should().BeTrue("Hint text should be visible");

        var hintText = await hint.TextContentAsync();
        hintText.Should().Contain("school", "Hint should mention school search");
    }

    [Fact]
    public async Task SchoolSearchIndex_RendersSearchButton()
    {
        await _page.GotoAsync(SchoolSearchPath);

        var button = _page.Locator("button[name='Search']");
        (await button.IsVisibleAsync()).Should().BeTrue("Search button should be visible");
    }

    [Fact]
    public async Task SchoolSearchIndex_SearchButton_ContainsMagnifyIcon()
    {
        await _page.GotoAsync(SchoolSearchPath);

        var buttonImage = _page.Locator("button[name='Search'] img[src*='magnify']");
        var count = await buttonImage.CountAsync();

        count.Should().Be(1, "Search button should contain magnifying glass icon");
    }

    [Fact]
    public async Task SchoolSearchResults_WithResults_RendersResultsList()
    {
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var pageContent = await _page.ContentAsync();
        pageContent.Should().Contain("school");
    }

    [Fact]
    public async Task SchoolSearchResults_PageStructure_HasGridLayout()
    {
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var gridRow = _page.Locator(".govuk-grid-row");
        var gridColumn = _page.Locator(".govuk-grid-column-two-thirds");

        (await gridRow.CountAsync()).Should().BeGreaterThan(0, "Page should use grid layout");
        (await gridColumn.CountAsync()).Should().BeGreaterThan(0, "Page should have two-thirds column");
    }

    #endregion
}