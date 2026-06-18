using Microsoft.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd;

[Collection("EndToEndTestsCollection")]
public class FindASchoolEndToEndTests(EndToEndTestsFixture fixture) : EndToEndTests(fixture)
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
    }

    [Fact]
    public async Task SearchForASchoolWithNoResults()
    {
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync("XXX");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await CurrentPageShouldNowBe(Routes.FindASchool(query: "XXX"));
        var errorMessage = Page.GetByRole(AriaRole.Alert);
        await Expect(errorMessage).ToBeVisibleAsync();
        await Expect(errorMessage).ToHaveTextAsync("There is a problem We could not find any schools matching your search criteria");
    }

    [Fact]
    public async Task SearchForASchoolWithMultiplePagesOfResultsAndNavigateBetweenPages()
    {
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync("School");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await CurrentPageShouldNowBe(Routes.FindASchool(query: "School"));
        await Expect(Page.GetByText("Showing 1 - 10 of 833 schools")).ToBeVisibleAsync();

        await Page.GetByText("Next page").ClickAsync();
        await CurrentPageShouldNowBe(Routes.FindASchool(query: "School", page: 2));
        await Page.GetByText("Showing 11 - 20 of 833 schools").IsVisibleAsync();

        await Page.GetByLabel("Page 84").ClickAsync();
        await CurrentPageShouldNowBe(Routes.FindASchool(query: "School", page: 84));
        await Page.GetByText("Showing 831 - 833 of 833 schools").IsVisibleAsync();

        await Page.GetByText("Previous page").ClickAsync();
        await CurrentPageShouldNowBe(Routes.FindASchool(query: "School", page: 83));
        await Page.GetByText("Showing 821 - 830 of 833 schools").IsVisibleAsync();

        await Page.GetByLabel("Page 1").ClickAsync();
        await CurrentPageShouldNowBe(Routes.FindASchool(query: "School", page: 1));
        await Page.GetByText("Showing 1 - 10 of 833 schools").IsVisibleAsync();

        await Page.GetByText("Abbeyfield School").ClickAsync();
        await CurrentPageShouldNowBe(Routes.School("138858"));
    }

    [Fact]
    public async Task SearchForASchoolViaUrn()
    {
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync("138858");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await CurrentPageShouldNowBe(Routes.School("138858"));
    }

    [Fact]
    public async Task SearchForASchoolWithASingleResult()
    {
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync("Abbeyfield School");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await CurrentPageShouldNowBe(Routes.School("138858"));
    }

    [Fact]
    public async Task SearchForASchoolUsingSearchSuggestions()
    {
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync("Abbeyfield");
        await Page.GetByText("Abbeyfield School").ClickAsync(new() { Force = true });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await CurrentPageShouldNowBe(Routes.School("138858"));
    }
}
