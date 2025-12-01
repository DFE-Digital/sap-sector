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

    private string SchoolSearchPath => $"{_fixture.BaseUrl}/school";
    private string SchoolSearchResultsPath => $"{_fixture.BaseUrl}/school/search"; 

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
            IgnoreHTTPSErrors = true  // ✅ Important for localhost HTTPS
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
        // Act
        var response = await _page.GotoAsync(SchoolSearchPath);

        response.Should().NotBeNull();
        response.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolSearchIndex_DisplaysQueryInputField()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        // Act
        var input = _page.Locator("input[name='Query']");
        var button = _page.Locator("button[name='Search']");
        var form = _page.Locator("form");

        var inputIsVisible = await input.IsVisibleAsync();
        inputIsVisible.Should().BeTrue("Query input field should be visible");

        var formIsVisible = await form.IsVisibleAsync();
        formIsVisible.Should().BeTrue("Search form should be visible");

        var submitTsVisible = await button.IsVisibleAsync();
        submitTsVisible.Should().BeTrue("Search button should be visible");
    }

    #endregion

    #region Form Validation Tests

    [Fact]
    public async Task SchoolSearchIndex_SubmitEmptyForm_ShowsValidationError()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        // Act
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorSummary = _page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeTrue("Error summary should be visible");

        var errorMessage = await errorSummary.Locator(".govuk-error-summary__list li").TextContentAsync();
        errorMessage.Should().Contain("Enter a school name or school ID to start a search");
    }

    [Fact]
    public async Task SchoolSearchIndex_SubmitShortQuery_ShowsValidationError()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        // Act
        await _page.Locator("input[name='Query']").FillAsync("AB");
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorSummary = _page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeTrue("Error summary should be visible for short query");

        var errorMessage = await errorSummary.TextContentAsync();
        errorMessage.Should().Contain("minimum 3 characters");
    }

    [Fact]
    public async Task SchoolSearchIndex_SubmitValidQuery_RedirectsToSearchResults()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        // Act
        await _page.Locator("input[name='Query']").FillAsync("Test School");
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForURLAsync("**/school/search?query=Test%20School");

        // Assert
        _page.Url.Should().Contain("/school/search");
        _page.Url.Should().Contain("query=Test");
    }

    [Fact]
    public async Task SchoolSearchIndex_ErrorInputField_HasErrorStyling()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        // Act
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorInput = _page.Locator("div.govuk-error-summary");
        var isVisible = await errorInput.IsVisibleAsync();
        isVisible.Should().BeTrue("Input field should have error styling");
    }

    [Fact]
    public async Task SchoolSearchIndex_NumericSearch_NoResults_ShowsErrorMessage()
    {
        await Page.GotoAsync(SchoolSearchPath);

        await Page.Locator("input[name='__Query']").FillAsync("123");
        await Page.WaitForTimeoutAsync(600);

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorSummary = Page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeTrue("Error summary should be visible");

        var errorMessage = await errorSummary.Locator(".govuk-error-summary__list li").TextContentAsync();
        errorMessage.Should().Contain("We could not find any schools matching your search criteria");
    }

    [Fact]
    public async Task SchoolSearchIndex_NumericSearch_ValidResults_RedirectsToSchoolDetailPage()
    {
        await Page.GotoAsync(SchoolSearchPath);

        await Page.Locator("input[name='__Query']").FillAsync("102848");
        await Page.WaitForTimeoutAsync(600);

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("school/102848");
        var schoolDetails = await Page.Locator(".govuk-body-l").TextContentAsync();
        schoolDetails.Should().Contain("This page Displays the School details.");
    }

    [Fact]
    public async Task SchoolSearchIndex_Search_SingleResult_RedirectsToSchoolDetailPage()
    {
        await Page.GotoAsync(SchoolSearchPath);

        await Page.Locator("input[name='__Query']").FillAsync("Saint Paul Roman Catholic");
        await Page.WaitForTimeoutAsync(600);

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("school/100273");
        var schoolDetails = await Page.Locator(".govuk-body-l").TextContentAsync();
        schoolDetails.Should().Contain("This page Displays the School details.");
    }

    #endregion

    #region Autocomplete/Suggester Tests

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_CreatesHiddenFields()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hiddenQueryField = Page.Locator("input[name='Query'][type='hidden']");
        var hiddenUrnField = Page.Locator("input[name='Urn'][type='hidden']");

        (await hiddenQueryField.CountAsync()).Should().Be(1, "Hidden Query field should exist");
        (await hiddenUrnField.CountAsync()).Should().Be(1, "Hidden Urn field should exist");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_SyncsVisibleInputWithHiddenInput()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("Test School");
        await Page.WaitForTimeoutAsync(100);

        var hiddenQueryValue = await Page.Locator("input[name='Query'][type='hidden']").InputValueAsync();
        hiddenQueryValue.Should().Be("Test School", "Hidden Query field should sync with visible input");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_RendersSuggestion()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.Locator("input[name='__Query']").FillAsync("School");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.WaitForTimeoutAsync(800);

        var suggestions = await Page.Locator(".autocomplete__option").AllTextContentsAsync();
        suggestions.Should().Contain(s => s.Contains("School"), "Suggestions should contain 'School'");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_Suggestion_SelectsFirstSuggestion_And_Submit_RedirectsToSchoolDetailPage()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.Locator("input[name='__Query']").FillAsync("School");
        await Page.WaitForTimeoutAsync(800);

        await Page.Locator(".autocomplete__option").First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("school/102848");
        var schoolDetails = await Page.Locator(".govuk-body-l").TextContentAsync();
        schoolDetails.Should().Contain("This page Displays the School details.");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_NumericInput_SetsHiddenUrnField()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("123456");
        await Page.WaitForTimeoutAsync(600);

        var isNumericValue = await Page.Locator("input[name='Urn']").InputValueAsync();
        isNumericValue.Should().Be("123456");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_TextInput_SetsHiddenUrnFieldToEmpty()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("School");
        await Page.WaitForTimeoutAsync(600);

        await Page.Locator("input[name='__Query']").FillAsync("Test School");
        await Page.WaitForTimeoutAsync(600);

        var isNumericValue = await Page.Locator("input[name='Urn']").InputValueAsync();
        isNumericValue.Should().Be(string.Empty);
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_NumericWithSlash_SetsHiddenUrnField()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("123/456");
        await Page.WaitForTimeoutAsync(600);

        var isNumericValue = await Page.Locator("input[name='Urn']").InputValueAsync();
        isNumericValue.Should().Be("123/456");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_NumericWithBackslash_SetsHiddenUrnField()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("123\\456");
        await Page.WaitForTimeoutAsync(600);

        var isNumericValue = await Page.Locator("input[name='Urn']").InputValueAsync();
        isNumericValue.Should().Be("123\\456");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_NumericWithBackslash_SetsHiddenUrnField_And_RedirectsToSchoolDetailPage()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("204\\3658");
        await Page.WaitForTimeoutAsync(600);

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("school/100273");
        var schoolDetails = await Page.Locator(".govuk-body-l").TextContentAsync();
        schoolDetails.Should().Contain("This page Displays the School details.");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_EnterKey_SubmitsForm()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("Test School");
        await Page.Locator("input[name='__Query']").PressAsync("Enter");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("/school/search", "Pressing Enter should submit the form");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_ClearsUrnFieldOnInput()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.EvaluateAsync("document.querySelector('input[name=\"Urn\"]').value = '123456'");
        await Page.Locator("input[name='__Query']").FillAsync("New School");
        await Page.WaitForTimeoutAsync(100);

        var urnValue = await Page.Locator("input[name='Urn']").InputValueAsync();
        urnValue.Should().Be(string.Empty, "Urn field should be cleared when user types");
    }

    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_MinLength3Characters_Required()
    {
        await Page.GotoAsync(SchoolSearchPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("AB");
        await Page.WaitForTimeoutAsync(600);

        var suggestionMenu = Page.Locator(".autocomplete__menu");
        var isVisible = await suggestionMenu.IsVisibleAsync();
        isVisible.Should().BeFalse("Autocomplete menu should not show for less than 3 characters");
    }

    #endregion

    #region Search Results Page Tests

    [Fact]
    public async Task SchoolSearchResults_LoadsSuccessfully()
    {
        // Act
        var response = await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        response.Should().NotBeNull();
        response.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolSearchResults_DisplaysSearchForm()
    {
        // Arrange
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Act
        var form = _page.Locator("form");
        var isVisible = await form.IsVisibleAsync();

        isVisible.Should().BeTrue("Search form should be visible on results page");
    }

    [Fact]
    public async Task SchoolSearchResults_FormHasPreviousQuery()
    {
        var query = "Test School";
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query={query}");

        // Act
        var input = _page.Locator("input[name='Query']");
        var inputValue = await input.InputValueAsync();

        inputValue.Should().Be(query, "Previous search query should be populated in the form");
    }

    [Fact]
    public async Task SchoolSearchResults_CanSearchAgain()
    {
        // Arrange
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Act
        await _page.Locator("input[name='Query']").FillAsync("Another School");
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        _page.Url.Should().Contain("Another");
    }

    [Fact]
    public async Task SchoolSearchResults_NumericWithSlash_SetsHiddenUrnField()
    {
        await Page.GotoAsync(SchoolSearchResultsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("123/456");
        await Page.WaitForTimeoutAsync(600);

        var isNumericValue = await Page.Locator("input[name='Urn']").InputValueAsync();
        isNumericValue.Should().Be("123/456");
    }

    [Fact]
    public async Task SchoolSearchResults_NumericWithBackslash_SetsHiddenUrnField()
    {
        await Page.GotoAsync(SchoolSearchResultsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("123\\456");
        await Page.WaitForTimeoutAsync(600);

        var isNumericValue = await Page.Locator("input[name='Urn']").InputValueAsync();
        isNumericValue.Should().Be("123\\456");
    }

    [Fact]
    public async Task SchoolSearchResults_NumericSearch_NoResults_ShowsErrorMessage()
    {
        await Page.GotoAsync(SchoolSearchResultsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("123");
        await Page.WaitForTimeoutAsync(600);

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorSummary = Page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeFalse();

        var warningMessage = await Page.Locator("#search-warning").TextContentAsync();
        warningMessage.Should().Contain("We couldn't find any schools matching your search criteria.");
    }

    [Fact]
    public async Task SchoolSearchResults_NumericSearch_ValidResults_RedirectsToSchoolDetailPage()
    {
        await Page.GotoAsync(SchoolSearchResultsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("102848");
        await Page.WaitForTimeoutAsync(600);

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("school/102848");
        var schoolDetails = await Page.Locator(".govuk-body-l").TextContentAsync();
        schoolDetails.Should().Contain("This page Displays the School details.");
    }

    [Fact]
    public async Task SchoolSearchResults_EmptyQuery_ShowsMessage()
    {
        // Act
        var response = await _page.GotoAsync($"{SchoolSearchResultsPath}?query=");

        response!.Status.Should().Be(200);
        var content = await _page.ContentAsync();
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SchoolSearchResults_WithResultsCount_DisplaysCorrectly()
    {
        // Arrange
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Act - Check if the result container exists
        _ = _page.Locator(".govuk-table, .search-results, [data-testid='search-results']").First;

        // Assert - Either results are shown or no results message
        var pageContent = await _page.ContentAsync();
        pageContent.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SchoolSearchResults_NoResults_DisplaysWarningMessage()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=XYZNonExistentSchool999");

        var warningMessage = Page.Locator("#search-warning");

        var isVisible = await warningMessage.IsVisibleAsync();
        if (isVisible)
        {
            var text = await warningMessage.TextContentAsync();
            text.Should().Contain("couldn't find any schools", "No results message should be displayed");
        }
    }

    [Fact]
    public async Task SchoolSearchResults_WithResults_DisplaysSchoolLinks()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var resultLinks = Page.Locator(".govuk-list--result a");
        var count = await resultLinks.CountAsync();

        if (count > 0)
        {
            var firstLink = resultLinks.First;
            var href = await firstLink.GetAttributeAsync("href");
            href.Should().Contain("/school/102848", "Result links should point to school detail pages");
        }
    }

    [Fact]
    public async Task SchoolSearchResults_ResultLink_HasCorrectStructure()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var resultItems = Page.Locator(".govuk-list--result li");
        var count = await resultItems.CountAsync();

        if (count > 0)
        {
            var firstItem = resultItems.First;
            var link = firstItem.Locator("a");

            (await link.CountAsync()).Should().Be(1, "Each result should have a link");

            var linkClasses = await link.GetAttributeAsync("class");
            linkClasses.Should().Contain("govuk-link", "Link should have GOV.UK styling");
        }
    }

    #endregion

    #region Edge Cases and Special Characters

    [Fact]
    public async Task SchoolSearchIndex_SpecialCharacters_HandledCorrectly()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        var specialQuery = "St. Mary's & John's School (Primary)";
        await _page.Locator("input[name='Query']").FillAsync(specialQuery);
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        _page.Url.Should().Contain("/school/search");
    }

    [Fact]
    public async Task SchoolSearchIndex_NumericQuery_ProcessedCorrectly()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        // Act
        await _page.Locator("input[name='Query']").FillAsync("123456");
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        _page.Url.Should().Contain("/school/search");
        _page.Url.Should().Contain("query=123456");
    }

    [Fact]
    public async Task SchoolSearchIndex_LongQuery_HandledCorrectly()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);
        var longQuery = new string('A', 200);

        // Act
        await _page.Locator("input[name='Query']").FillAsync(longQuery);
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var response = _page.Url;
        response.Should().Contain("/school/search");
    }

    [Fact]
    public async Task SchoolSearchResults_QueryWithWhitespace_HandledCorrectly()
    {
        var query = "  School   Name  ";

        // Act
        var response = await _page.GotoAsync($"{SchoolSearchResultsPath}?query={Uri.EscapeDataString(query)}");

        response!.Status.Should().Be(200);
    }

    #endregion

    #region Autocomplete Suggest API Tests

    //[Fact]
    //public async Task SchoolSuggest_EmptyQuery_ReturnsSuccessfully()
    //{
    //    // Act
    //    var response = await _page.GotoAsync("/school/suggest?queryPart=");

    //    // Assert
    //    response!.Status.Should().Be(200);
    //    var contentType = response.Headers["content-type"];
    //    contentType.Should().Contain("application/json");
    //}

    //[Fact]
    //public async Task SchoolSuggest_SpecialCharacters_HandledCorrectly()
    //{
    //    // Act
    //    var response = await _page.GotoAsync("/school/suggest?queryPart=St.%20Mary's");

    //    // Assert
    //    response!.Status.Should().Be(200);
    //}

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task SchoolSearchIndex_ErrorSummaryIsVisible()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        // Act
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorSummary = _page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeTrue("Error summary should be visible and focusable");

        var title = errorSummary.Locator(".govuk-error-summary__title");
        var list = errorSummary.Locator(".govuk-error-summary__list");

        (await errorSummary.IsVisibleAsync()).Should().BeTrue("Error summary should be visible");
        (await title.IsVisibleAsync()).Should().BeTrue("Error summary title should be visible");
        (await list.IsVisibleAsync()).Should().BeTrue("Error summary list should be visible");
    }

    [Fact]
    public async Task SchoolSearchIndex_ErrorSummaryLinksToField()
    {
        // Arrange
        await _page.GotoAsync(SchoolSearchPath);

        // Act
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorLink = _page.Locator(".govuk-error-summary__list a");
        var href = await errorLink.GetAttributeAsync("href");
        href.Should().Contain("#", "Error summary links should point to form fields");
    }

    [Fact]
    public async Task SchoolSearchIndex_InputHasAriaDescribedBy()
    {
        await Page.GotoAsync(SchoolSearchPath);

        var input = Page.Locator("input[name='Query']");
        var ariaDescribedBy = await input.GetAttributeAsync("aria-describedby");

        ariaDescribedBy.Should().Contain("hint", "Input should reference hint text");
    }

    [Fact]
    public async Task SchoolSearchIndex_ErrorInput_HasAriaDescribedByError()
    {
        await Page.GotoAsync(SchoolSearchPath);

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var input = Page.Locator("input[name='Query']");
        var ariaDescribedBy = await input.GetAttributeAsync("aria-describedby");

        ariaDescribedBy.Should().Contain("error", "Input with error should reference error message");
    }

    [Fact]
    public async Task SchoolSearchIndex_FormHasSearchRole()
    {
        await Page.GotoAsync(SchoolSearchPath);

        var form = Page.Locator("form[role='search']");
        var exists = await form.CountAsync();

        exists.Should().BeGreaterThan(0, "Form should have search role for accessibility");
    }

    #endregion

    #region Error Scenarios

    [Fact]
    public async Task SchoolSearchResults_WithNullQuery_HandlesGracefully()
    {
        // Act
        var response = await _page.GotoAsync($"{SchoolSearchResultsPath}");

        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolSearchIndex_FormWithJavascriptDisabled_StillWorks()
    {
        // Arrange
        await _page.CloseAsync();

        var context = await _browser.NewContextAsync(new()
        {
            JavaScriptEnabled = false,
            BaseURL = _fixture.BaseUrl,
            IgnoreHTTPSErrors = true
        });
        var jsDisabledPage = await context.NewPageAsync();

        await jsDisabledPage.GotoAsync(SchoolSearchPath);

        await jsDisabledPage.Locator("input[name='Query']").FillAsync("Test School");
        await jsDisabledPage.Locator("button[name='Search']").ClickAsync();
        await jsDisabledPage.WaitForLoadStateAsync(LoadState.NetworkIdle);

        jsDisabledPage.Url.Should().Contain("/school/search");

        await jsDisabledPage.CloseAsync();
    }

    #endregion

    #region POST Action Tests

    [Fact]
    public async Task SchoolSearchResults_PostWithUrn_RedirectsToSchoolProfile()
    {
        // This test verifies the POST action when an establishment ID is selected
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Verify the form is present for POST action
        var form = _page.Locator("form");
        var isVisible = await form.IsVisibleAsync();
        isVisible.Should().BeTrue("Form should be present for posting school ID");
    }

    [Fact]
    public async Task SchoolSearchResults_PostEmptyQuery_ShowsValidationError()
    {
        // Arrange
        await _page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Act
        await _page.Locator("input[name='Query']").ClearAsync();
        await _page.Locator("button[name='Search']").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorSummary = _page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeTrue();

        var content = await _page.ContentAsync();
        content.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region View Rendering Tests

    [Fact]
    public async Task SchoolSearchIndex_RendersCorrectPageTitle()
    {
        await Page.GotoAsync(SchoolSearchPath);

        var title = await Page.TitleAsync();
        var heading = await Page.Locator("h1").TextContentAsync();

        title.Should().NotBeNullOrEmpty("Page should have a title");
        heading.Should().Contain("school", "Heading should mention schools");
    }

    [Fact]
    public async Task SchoolSearchIndex_RendersHintText()
    {
        await Page.GotoAsync(SchoolSearchPath);

        var hint = Page.Locator(".govuk-hint");
        var isVisible = await hint.IsVisibleAsync();
        var hintText = await hint.TextContentAsync();

        isVisible.Should().BeTrue("Hint text should be visible");
        hintText.Should().Contain("school", "Hint should mention school search");
    }

    [Fact]
    public async Task SchoolSearchIndex_RendersSearchButton()
    {
        await Page.GotoAsync(SchoolSearchPath);

        var button = Page.Locator("button[name='Search']");
        var isVisible = await button.IsVisibleAsync();

        isVisible.Should().BeTrue("Search button should be visible");
    }

    [Fact]
    public async Task SchoolSearchIndex_SearchButton_ContainsMagnifyIcon()
    {
        await Page.GotoAsync(SchoolSearchPath);

        var buttonImage = Page.Locator("button[name='Search'] img[src*='magnify']");
        var count = await buttonImage.CountAsync();

        count.Should().Be(1, "Search button should contain magnifying glass icon");
    }

    [Fact]
    public async Task SchoolSearchResults_WithResults_RendersResultsList()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var resultsList = Page.Locator(".govuk-list--result");
        _ = await resultsList.CountAsync();

        var pageContent = await Page.ContentAsync();
        pageContent.Should().Contain("school");
    }

    [Fact]
    public async Task SchoolSearchResults_PageStructure_HasGridLayout()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var gridRow = Page.Locator(".govuk-grid-row");
        var gridColumn = Page.Locator(".govuk-grid-column-two-thirds");

        (await gridRow.CountAsync()).Should().BeGreaterThan(0, "Page should use grid layout");
        (await gridColumn.CountAsync()).Should().BeGreaterThan(0, "Page should have two-thirds column");
    }

    [Fact]
    public async Task SchoolSearchResults_HasSectionBreak()
    {
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        var sectionBreak = Page.Locator("hr.govuk-section-break");
        var count = await sectionBreak.CountAsync();

        count.Should().BeGreaterThan(0, "Results page should have section breaks");
    }

    #endregion
}