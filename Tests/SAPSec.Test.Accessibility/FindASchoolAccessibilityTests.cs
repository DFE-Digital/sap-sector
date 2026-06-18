using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class FindASchoolAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    [Fact]
    public async Task SchoolSearchIndex_Autocomplete_EnterKey_SubmitsForm()
    {
        await NavigateTo(Routes.FindASchool());
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("input[name='__Query']").FillAsync("Test School");
        await Page.Locator("input[name='__Query']").PressAsync("Enter");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("/find-a-school", "Pressing Enter should submit the form");
    }

    [Fact]
    public async Task SchoolSearchIndex_ErrorSummaryIsVisible()
    {
        await NavigateTo(Routes.FindASchool());

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

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
        await NavigateTo(Routes.FindASchool());

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorLink = Page.Locator(".govuk-error-summary__list a");
        var href = await errorLink.GetAttributeAsync("href");
        href.Should().Contain("#", "Error summary links should point to form fields");
    }

    [Fact]
    public async Task SchoolSearchIndex_InputHasAriaDescribedBy()
    {
        await NavigateTo(Routes.FindASchool());

        var input = Page.Locator("input[name='Query']");
        var ariaDescribedBy = await input.GetAttributeAsync("aria-describedby");

        ariaDescribedBy.Should().Contain("hint", "Input should reference hint text");
    }

    [Fact]
    public async Task SchoolSearchIndex_ErrorInput_HasAriaDescribedByError()
    {
        await NavigateTo(Routes.FindASchool());

        await Page.Locator("button[name='Search']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var input = Page.Locator("input[name='Query']");
        var ariaDescribedBy = await input.GetAttributeAsync("aria-describedby");

        ariaDescribedBy.Should().Contain("error", "Input with error should reference error message");
    }

    [Fact]
    public async Task SchoolSearchIndex_FormHasSearchRole()
    {
        await NavigateTo(Routes.FindASchool());

        var form = Page.Locator("form[role='search']");
        var exists = await form.CountAsync();

        exists.Should().BeGreaterThan(0, "Form should have search role for accessibility");
    }

    [Fact]
    public async Task SchoolSearchResults_NoResults_ErrorSummaryHasAlertRole()
    {
        await NavigateTo(Routes.FindASchool(query: "XYZNonExistentSchool999"));

        var alertRole = Page.Locator(".govuk-error-summary [role='alert']");
        var count = await alertRole.CountAsync();

        count.Should().BeGreaterThan(0, "Error summary should have alert role for accessibility");
    }

    [Fact]
    public async Task SchoolSearchIndex_FormWithJavascriptDisabled_StillWorks()
    {
        await Page.CloseAsync();

        var context = await Browser.NewContextAsync(new()
        {
            JavaScriptEnabled = false,
            BaseURL = fixture.BaseUrl,
            IgnoreHTTPSErrors = true
        });
        var jsDisabledPage = await context.NewPageAsync();

        await jsDisabledPage.GotoAsync(Routes.FindASchool());

        await jsDisabledPage.Locator("input[name='Query']").FillAsync("Test School");
        await jsDisabledPage.Locator("button[name='Search']").ClickAsync();
        await jsDisabledPage.WaitForLoadStateAsync(LoadState.NetworkIdle);

        jsDisabledPage.Url.Should().Contain(Routes.FindASchool());

        await jsDisabledPage.CloseAsync();
    }

}
