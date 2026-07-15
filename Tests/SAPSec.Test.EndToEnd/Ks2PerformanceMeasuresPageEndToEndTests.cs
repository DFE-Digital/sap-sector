using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using System.Text.RegularExpressions;
using Xunit;

namespace SAPSec.Test.EndToEnd;

[Collection("EndToEndTestsCollection")]
public class Ks2PerformanceMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string UrlPattern = @"\d{6}";
    private const string MeetingExpectedStandardHeaderText = "Meeting expected standard in reading, writing and maths";

    private static readonly Routes.Primary PrimarySchoolRoute = Routes.PrimarySchool("145140");

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(PrimarySchoolRoute.Overview);
        await Page.GetByText("KS2", new() { Exact = true }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.KS2);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        var panel = section.GetByRole(AriaRole.Tabpanel);

        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2024 to 2025" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });

        var showYearByYearButton = section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" });
        var showCurrentYearButton = section.GetByRole(AriaRole.Button, new() { Name = "Show 2024 to 2025" });

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await Expect(showYearByYearButton).ToBeVisibleAsync();
        await Expect(showCurrentYearButton).ToBeHiddenAsync();

        await showYearByYearButton.ClickAsync();

        await Expect(currentYearHeader).ToBeHiddenAsync();
        await Expect(yearByYearHeader).ToBeVisibleAsync();

        await Expect(showCurrentYearButton).ToBeVisibleAsync();
        await Expect(showYearByYearButton).ToBeHiddenAsync();

        await showCurrentYearButton.ClickAsync();

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await Expect(showYearByYearButton).ToBeVisibleAsync();
        await Expect(showCurrentYearButton).ToBeHiddenAsync();
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        var topPerfomersTab = section.GetByRole(AriaRole.Tab, new() { Name = "Top performers" });
        await topPerfomersTab.ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var values = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(values).ToBePercentageValuesHavingCount(3);

        var schools = await table.GetTableColumnAsync("School");
        await Expect(schools).ToHaveCountAsync(3);

        var schoolLinks = schools.GetByRole(AriaRole.Link);
        await schoolLinks.Nth(0).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(PrimarySchoolRoute.SimilarSchoolComparison(UrlPattern)));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ViewTableView()
    {
        var section = await GetSection(MeetingExpectedStandardHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var previous2 = await table.GetTableColumnAsync("2022 to 2023");
        await Expect(previous2).ToBePercentageValuesHavingCount(4);

        var previous = await table.GetTableColumnAsync("2023 to 2024");
        await Expect(previous).ToBePercentageValuesHavingCount(4);

        var current = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(current).ToBePercentageValuesHavingCount(4);
    }

    private async Task<ILocator> GetSection(string headerText)
    {
        var section = Page.GetByLabel(MeetingExpectedStandardHeaderText);
        await Expect(section).ToBeVisibleAsync();

        return section;
    }
}