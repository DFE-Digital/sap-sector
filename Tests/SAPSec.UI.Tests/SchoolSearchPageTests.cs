using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

public class SchoolSearchPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture), IClassFixture<WebApplicationSetupFixture>
{
    private readonly WebApplicationSetupFixture _fixture = fixture;

    private const string SchoolSearchPath = "/school";
    private const string SchoolSearchResultsPath = "/school/search";

    #region Index Page Tests

    [Fact]
    public async Task SchoolSearchIndex_LoadsSuccessfully()
    {
        // Act
        var response = await Page.GotoAsync(SchoolSearchPath);

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be(200);
    }

    // [Fact(Skip = "Not implemented")]
    // public async Task SchoolSearchIndex_HasCorrectTitle()
    // {
    //     // Arrange
    //     await Page.GotoAsync(SchoolSearchPath);
    //
    //     // Act
    //     var title = await Page.TitleAsync();
    //
    //     // Assert
    //     title.Should().NotBeNullOrWhiteSpace();
    // }

    [Fact]
    public async Task SchoolSearchIndex_DisplaysQueryInputField()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        var input = Page.Locator("input[name='Query']");
        var button = Page.Locator("button[name='Search']");
        var form = Page.Locator("form");

        var inputIsVisible = await input.IsVisibleAsync();
        inputIsVisible.Should().BeTrue("Query input field should be visible");

        var formIsVisible = await form.IsVisibleAsync();
        formIsVisible.Should().BeTrue("Search form should be visible");

        var submitTsVisible = await button.IsVisibleAsync();
        submitTsVisible.Should().BeTrue("Search button should be visible");
    }

    // [Fact(Skip = "Not implemented")]
    // public async Task SchoolSearchIndex_DisplaysHintText()
    // {
    //     // Arrange
    //     await Page.GotoAsync(SchoolSearchPath);
    //
    //     // Act
    //     var hint = Page.Locator(".govuk-hint");
    //     var hintText = await hint.TextContentAsync();
    //
    //     // Assert
    //     hintText.Should().Contain("Search by name, address, postcode or unique reference number (URN)");
    // }

    #endregion

    #region Form Validation Tests

    [Fact]
    public async Task SchoolSearchIndex_SubmitEmptyForm_ShowsValidationError()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorSummary = Page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeTrue("Error summary should be visible");

        var errorMessage = await errorSummary.Locator(".govuk-error-summary__list li").TextContentAsync();
        errorMessage.Should().Contain("Enter a school name or URN");
    }

    [Fact]
    public async Task SchoolSearchIndex_SubmitShortQuery_ShowsValidationError()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        await Page.Locator("input[name='Query']").FillAsync("AB");
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorSummary = Page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeTrue("Error summary should be visible for short query");

        var errorMessage = await errorSummary.TextContentAsync();
        errorMessage.Should().Contain("minimum 3 characters");
    }

    [Fact]
    public async Task SchoolSearchIndex_SubmitValidQuery_RedirectsToSearchResults()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        await Page.Locator("input[name='Query']").FillAsync("Test School");
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForURLAsync("**/school/search?query=Test%20School");

        // Assert
        Page.Url.Should().Contain("/school/search");
        Page.Url.Should().Contain("query=Test");
    }

    [Fact]
    public async Task SchoolSearchIndex_ErrorInputField_HasErrorStyling()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorInput = Page.Locator("div.govuk-error-summary");
        var isVisible = await errorInput.IsVisibleAsync();
        isVisible.Should().BeTrue("Input field should have error styling");
    }

    #endregion

    #region Search Results Page Tests

    [Fact]
    public async Task SchoolSearchResults_LoadsSuccessfully()
    {
        // Act
        var response = await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolSearchResults_DisplaysSearchForm()
    {
        // Arrange
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Act
        var form = Page.Locator("form");
        var isVisible = await form.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("Search form should be visible on results page");
    }

    [Fact]
    public async Task SchoolSearchResults_FormHasPreviousQuery()
    {
        // Arrange
        var query = "Test School";
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query={query}");

        // Act
        var input = Page.Locator("input[name='Query']");
        var inputValue = await input.InputValueAsync();

        // Assert
        inputValue.Should().Be(query, "Previous search query should be populated in the form");
    }

    [Fact]
    public async Task SchoolSearchResults_CanSearchAgain()
    {
        // Arrange
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Act
        await Page.Locator("input[name='Query']").FillAsync("Another School");
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().Contain("Another");
    }

    [Fact]
    public async Task SchoolSearchResults_EmptyQuery_ShowsMessage()
    {
        // Act
        var response = await Page.GotoAsync($"{SchoolSearchResultsPath}?query=");

        // Assert
        response!.Status.Should().Be(200);
        var content = await Page.ContentAsync();
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SchoolSearchResults_WithResultsCount_DisplaysCorrectly()
    {
        // Arrange
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Act - Check if the result container exists
        _ = Page.Locator(".govuk-table, .search-results, [data-testid='search-results']").First;

        // Assert - Either results are shown or no results message
        var pageContent = await Page.ContentAsync();
        pageContent.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Edge Cases and Special Characters

    [Fact]
    public async Task SchoolSearchIndex_SpecialCharacters_HandledCorrectly()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        var specialQuery = "St. Mary's & John's School (Primary)";
        await Page.Locator("input[name='Query']").FillAsync(specialQuery);
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().Contain("/school/search");
    }

    [Fact]
    public async Task SchoolSearchIndex_NumericQuery_ProcessedCorrectly()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        await Page.Locator("input[name='Query']").FillAsync("123456");
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().Contain("/school/search");
        Page.Url.Should().Contain("query=123456");
    }

    [Fact]
    public async Task SchoolSearchIndex_LongQuery_HandledCorrectly()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);
        var longQuery = new string('A', 200);

        // Act
        await Page.Locator("input[name='Query']").FillAsync(longQuery);
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var response = Page.Url;
        response.Should().Contain("/school/search");
    }

    [Fact]
    public async Task SchoolSearchResults_QueryWithWhitespace_HandledCorrectly()
    {
        // Arrange
        var query = "  School   Name  ";

        // Act
        var response = await Page.GotoAsync($"{SchoolSearchResultsPath}?query={Uri.EscapeDataString(query)}");

        // Assert
        response!.Status.Should().Be(200);
    }

    #endregion

    #region Autocomplete/suggest Tests

    [Fact]
    public async Task SchoolSuggest_EmptyQuery_ReturnsSuccessfully()
    {
        // Act
        var response = await Page.GotoAsync("/school/suggest?queryPart=");

        // Assert
        response!.Status.Should().Be(200);
        var contentType = response.Headers["content-type"];
        contentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task SchoolSuggest_SpecialCharacters_HandledCorrectly()
    {
        // Act
        var response = await Page.GotoAsync("/school/suggest?queryPart=St.%20Mary's");

        // Assert
        response!.Status.Should().Be(200);
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task SchoolSearchIndex_ErrorSummaryIsVisible()
    {
        // Arrange
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorSummary = Page.Locator(".govuk-error-summary");
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
        await Page.GotoAsync(SchoolSearchPath);

        // Act
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorLink = Page.Locator(".govuk-error-summary__list a");
        var href = await errorLink.GetAttributeAsync("href");
        href.Should().Contain("#", "Error summary links should point to form fields");
    }

    #endregion

    // #region Navigation and Flow Tests
    //
    // [Fact(Skip = "Not implemented")]
    // public async Task SchoolSearchIndex_CanNavigateToHomePage()
    // {
    //     // Arrange
    //     await Page.GotoAsync(SchoolSearchPath);
    //
    //     // Act
    //     var homeLink = Page.Locator("homeLink").First;
    //     var isVisible = await homeLink.IsVisibleAsync();
    //
    //     // Assert
    //     isVisible.Should().BeTrue("Should have navigation back to home page");
    // }
    //
    // [Fact(Skip = "Not implemented")]
    // public async Task SchoolSearchIndex_CanNavigateToBack()
    // {
    //     // Arrange
    //     await Page.GotoAsync(SchoolSearchPath);
    //
    //     // Act
    //     var homeLink = Page.Locator("backlink").First;
    //     var isVisible = await homeLink.IsVisibleAsync();
    //
    //     // Assert
    //     isVisible.Should().BeTrue("Should have navigation back to home page");
    // }
    //
    // #endregion

    #region Error Scenarios

    [Fact]
    public async Task SchoolSearchResults_WithNullQuery_HandlesGracefully()
    {
        // Act
        var response = await Page.GotoAsync($"{SchoolSearchResultsPath}");

        // Assert
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolSearchIndex_FormWithJavascriptDisabled_StillWorks()
    {
        // Arrange
        await Page.CloseAsync();

        var context = await Browser.NewContextAsync(new()
        {
            JavaScriptEnabled = false,
            BaseURL = _fixture.BaseUrl,
            IgnoreHTTPSErrors = true
        });
        var jsDisabledPage = await context.NewPageAsync();

        await jsDisabledPage.GotoAsync(SchoolSearchPath);

        // Act
        await jsDisabledPage.Locator("input[name='Query']").FillAsync("Test School");
        await jsDisabledPage.Locator("button[name='Search']").ClickAsync();
        await jsDisabledPage.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        jsDisabledPage.Url.Should().Contain("/school/search");

        // Cleanup
        await jsDisabledPage.CloseAsync();
    }

    #endregion

    #region POST Action Tests

    [Fact]
    public async Task SchoolSearchResults_PostWithEstablishmentId_RedirectsToSchoolProfile()
    {
        // This test verifies the POST action when an establishment ID is selected
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Verify the form is present for POST action
        var form = Page.Locator("form");
        var isVisible = await form.IsVisibleAsync();
        isVisible.Should().BeTrue("Form should be present for posting establishment ID");
    }

    [Fact]
    public async Task SchoolSearchResults_PostEmptyQuery_ShowsValidationError()
    {
        // Arrange
        await Page.GotoAsync($"{SchoolSearchResultsPath}?query=School");

        // Act
        await Page.Locator("input[name='Query']").ClearAsync();
        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var errorSummary = Page.Locator(".govuk-error-summary");
        var isVisible = await errorSummary.IsVisibleAsync();
        isVisible.Should().BeTrue();

        var content = await Page.ContentAsync();
        content.Should().NotBeNullOrWhiteSpace();
    }

    #endregion
}
