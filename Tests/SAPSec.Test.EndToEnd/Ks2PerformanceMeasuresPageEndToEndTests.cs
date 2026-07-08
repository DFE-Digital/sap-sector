using FluentAssertions;
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
    private const string PercentageValuePattern = @"\d\d%";
    private static readonly Routes.Primary PrimarySchoolRoute = Routes.PrimarySchool("145140");

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(PrimarySchoolRoute.Home);
        await Page.GetByText("KS2", new() { Exact = true }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.KS2);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = Page.GetByLabel("Meeting expected standard in reading, writing and maths");
        await Expect(section).ToBeVisibleAsync();

        await Expect(section).ToMatchScreenshotAsync("meeting-expected-standard-rwm-current-year");

        var charts = section.GetByRole(AriaRole.Tab, new() { Name = "Charts" });
        await charts.ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2024 to 2025" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });

        var showYearByYearButton = section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" });
        var showCurrentYearButton = section.GetByRole(AriaRole.Button, new() { Name = "Show 2024 to 2025" });

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await Expect(showYearByYearButton).ToBeVisibleAsync();
        await Expect(showCurrentYearButton).ToBeHiddenAsync();

        await showYearByYearButton.ClickAsync();
        // Click away to clear focus state on button
        await section.ClickAsync();

        await Expect(section).ToMatchScreenshotAsync("meeting-expected-standard-rwm-year-by-year");

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
        var section = Page.GetByLabel("Meeting expected standard in reading, writing and maths");
        await Expect(section).ToBeVisibleAsync();

        var topPerfomers = section.GetByRole(AriaRole.Tab, new() { Name = "Top performers" });
        await topPerfomers.ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var values = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(values).ToHaveCountAsync(3);
        var text = await values.AllTrimmedTextContentsAsync();
        text.Should().AllSatisfy(x => x.Should().MatchRegex(PercentageValuePattern));

        var schools = await table.GetTableColumnAsync("School");
        await Expect(schools).ToHaveCountAsync(3);

        var schoolLinks = schools.GetByRole(AriaRole.Link);
        await schoolLinks.Nth(0).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(PrimarySchoolRoute.SimilarSchoolComparison(UrlPattern)));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomers).ToBeVisibleAsync();
        await topPerfomers.ClickAsync();

        var similarSchoolsLink = section.GetByText("See all similar schools");
        await similarSchoolsLink.ClickAsync();

        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ViewTableView()
    {
        var section = Page.GetByLabel("Meeting expected standard in reading, writing and maths");
        await Expect(section).ToBeVisibleAsync();

        var tableView = section.GetByRole(AriaRole.Tab, new() { Name = "Table" });
        await tableView.ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var previous2 = await table.GetTableColumnAsync("2022 to 2023");
        await Expect(previous2).ToHaveCountAsync(4);
        var previous2Values = await previous2.AllTrimmedTextContentsAsync();
        previous2Values.Should().AllSatisfy(x => x.Should().MatchRegex(PercentageValuePattern));

        var previous = await table.GetTableColumnAsync("2023 to 2024");
        await Expect(previous).ToHaveCountAsync(4);
        var previousValues = await previous.AllTrimmedTextContentsAsync();
        previousValues.Should().AllSatisfy(x => x.Should().MatchRegex(PercentageValuePattern));

        var current = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(current).ToHaveCountAsync(4);
        var currentValues = await current.AllTrimmedTextContentsAsync();
        currentValues.Should().AllSatisfy(x => x.Should().MatchRegex(PercentageValuePattern));
    }
}