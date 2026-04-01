using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SchoolKs4CoreSubjectsPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string PagePath = "/school/105574/ks4-core-subjects";

    [Fact]
    public async Task Ks4CoreSubjects_LoadsEnglishLanguagePage()
    {
        var response = await Page.GotoAsync(PagePath);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "KS4 core subject GCSE results" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "English language" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "English literature" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Ks4CoreSubjects_ShowsGradeFilterAndActiveNavigation()
    {
        await Page.GotoAsync(PagePath);

        await Expect(Page.Locator("#englishLanguageGrade")).ToBeVisibleAsync();
        await Expect(Page.Locator("#englishLanguageCharacteristic")).ToBeDisabledAsync();
        await Expect(Page.Locator(".app-side-navigation__item--selected").GetByText("KS4 core subjects")).ToBeVisibleAsync();
    }
}
