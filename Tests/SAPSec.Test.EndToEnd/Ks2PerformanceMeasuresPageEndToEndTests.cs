using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd;

[Collection("EndToEndTestsCollection")]
public class Ks2PerformanceMeasuresPageEndToEndTests(EndToEndTestsFixture fixture) : EndToEndTests(fixture)
{
    private static readonly Routes.Primary PrimarySchoolRoute = Routes.PrimarySchool("100749");

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(PrimarySchoolRoute.Home);
        await Page.GetByText("KS2", new() { Exact = true }).ClickAsync();
        await CurrentPageShouldNowBe(PrimarySchoolRoute.KS2);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = Page.GetByLabel("Meeting expected standard in reading, writing and maths");
        await section.ShouldBeVisibleAsync();

        var charts = section.GetByRole(AriaRole.Tab, new() { Name = "Charts" });
        await charts.ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2024 to 2025" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });
        var showYearByYear = section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" });
        var showCurrentYear = section.GetByRole(AriaRole.Button, new() { Name = "Show 2024 to 2025" });

        await currentYearHeader.ShouldBeVisibleAsync();
        await yearByYearHeader.ShouldBeHiddenAsync();
        await showYearByYear.ShouldBeVisibleAsync();
        await showCurrentYear.ShouldBeHiddenAsync();

        await showYearByYear.ClickAsync();

        await currentYearHeader.ShouldBeHiddenAsync();
        await yearByYearHeader.ShouldBeVisibleAsync();
        await showCurrentYear.ShouldBeVisibleAsync();
        await showYearByYear.ShouldBeHiddenAsync();

        await showCurrentYear.ClickAsync();

        await currentYearHeader.ShouldBeVisibleAsync();
        await yearByYearHeader.ShouldBeHiddenAsync();
        await showYearByYear.ShouldBeVisibleAsync();
        await showCurrentYear.ShouldBeHiddenAsync();
    }
}