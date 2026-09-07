using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Primary;

[Collection("EndToEndTestsCollection")]
public class RiseResourcesPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string Urn = "101206";
    private static readonly Routes.Primary SchoolRoute = Routes.PrimarySchool(Urn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(Urn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SchoolRoute.Overview);
    }

    [Fact]
    public async Task RiseResources_IsReachableFromTheSchoolNavigation()
    {
        await Page.GetByRole(AriaRole.Link, new() { Name = "RISE resources", Exact = true }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(SchoolRoute.RiseResources);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1, Name = PageTitles.RiseResources }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByTestId("rise-resources-intro")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task RiseResources_RendersContentsAndCategorySectionsFromTheContentFile()
    {
        await NavigateTo(SchoolRoute.RiseResources);

        await Expect(Page.GetByTestId("rise-resources-category")).ToHaveTextAsync(
        [
            "Performance and attendance",
            "Pupil characteristics",
            "Wider school"
        ]);

        var contentsLinkCount = await Page.GetByTestId("rise-resources-contents")
            .GetByRole(AriaRole.Link).CountAsync();
        var subCategoryCount = await Page.GetByTestId("rise-resources-subcategory").CountAsync();

        contentsLinkCount.Should().Be(subCategoryCount).And.BeGreaterThan(0);

        await Expect(Page.GetByTestId("rise-resource-title").First).ToHaveAttributeAsync("target", "_blank");
    }
}
