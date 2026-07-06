using Microsoft.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

[Collection("EndToEndTestsCollection")]
public class Ks2PerformanceMeasuresPageEndToEndTests(EndToEndTestsFixture fixture) : EndToEndTests(fixture)
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
}