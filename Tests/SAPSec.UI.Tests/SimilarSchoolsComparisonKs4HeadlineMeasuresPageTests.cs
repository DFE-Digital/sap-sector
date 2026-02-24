using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SimilarSchoolsComparisonKs4HeadlineMeasuresPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string Path = "/school/108088/view-similar-schools/137621/Ks4HeadlineMeasures";

    [Fact]
    public async Task Ks4HeadlineMeasuresComparison_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(Path);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task Ks4HeadlineMeasuresComparison_ShowsExpectedBarsAndColours()
    {
        await Page.GotoAsync(Path);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var chartRows = Page.Locator(".app-ks4-bar-chart .app-ks4-bar-row");
        (await chartRows.CountAsync()).Should().Be(3);

        (await Page.Locator(".app-ks4-bar--selected-school").CountAsync()).Should().Be(1);
        (await Page.Locator(".app-ks4-bar--this-school").CountAsync()).Should().Be(1);
        (await Page.Locator(".app-ks4-bar--england-overall").CountAsync()).Should().Be(1);
    }
}
