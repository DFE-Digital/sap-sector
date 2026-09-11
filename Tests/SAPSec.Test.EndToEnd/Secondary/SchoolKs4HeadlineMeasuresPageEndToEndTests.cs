using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using System.Text.RegularExpressions;
using Xunit;

namespace SAPSec.Test.EndToEnd.Secondary;

[Collection("EndToEndTestsCollection")]
public class SchoolKs4HeadlineMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string UrlPattern = @"\d{6}";
    private const string Attainment8HeaderText = "Attainment 8";
    private const string EnglishMathsHeaderText = "Grade achieved in English and maths GCSEs";
    private const string DestinationsHeaderText = "Staying in education or entering employment";

    private const string Urn = "100052";
    private static readonly Routes.Secondary SecondarySchoolRoute = Routes.SecondarySchool(Urn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(Urn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.Overview);
        await Page.GetByText("KS4 headline measures", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.KS4HeadlineMeasures);
    }

    [Fact]
    public async Task Attainment8_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(Attainment8HeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearPanel = section.Locator("[data-content-toggle-name=\"2024 to 2025\"]");
        var yearByYearPanel = section.Locator("[data-content-toggle-name=\"Year by year\"]");
        var toggleButton = section.Locator(".app-content-toggle__header button[type=\"button\"]");

        await Expect(currentYearPanel).ToBeVisibleAsync();
        await Expect(yearByYearPanel).ToBeHiddenAsync();

        await toggleButton.ClickAsync();

        await Expect(currentYearPanel).ToBeHiddenAsync();
        await Expect(yearByYearPanel).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Attainment8_ViewTableView()
    {
        var section = await GetSection(Attainment8HeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        foreach (var heading in new[] { "2022 to 2023", "2023 to 2024", "2024 to 2025" })
        {
            var values = await table.GetTableColumnAsync(heading);
            await Expect(values).ToHaveCountAsync(4);
            (await values.AllTrimmedTextContentsAsync()).Should().AllSatisfy(v =>
                (v == "No available data" || decimal.TryParse(v, out _)).Should().BeTrue());
        }
    }

    [Fact]
    public async Task Attainment8_ViewTopPerfomers()
    {
        var section = await GetSection(Attainment8HeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Top performing similar schools" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var values = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(values).ToHaveCountAsync(3);
        (await values.AllTrimmedTextContentsAsync()).Should().AllSatisfy(v => decimal.TryParse(v, out _).Should().BeTrue());
    }

    [Fact]
    public async Task EnglishMaths_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(EnglishMathsHeaderText);
        var panel = section.GetByRole(AriaRole.Tabpanel);

        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearPanel = section.Locator("[data-content-toggle-name=\"2024 to 2025\"]");
        var yearByYearPanel = section.Locator("[data-content-toggle-name=\"Year by year\"]");
        var toggleButton = section.Locator(".app-content-toggle__header button[type=\"button\"]");


        await Expect(currentYearPanel).ToBeVisibleAsync();
        await Expect(yearByYearPanel).ToBeHiddenAsync();

        await Expect(toggleButton).ToBeVisibleAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "false");

        await toggleButton.ClickAsync();

        await Expect(currentYearPanel).ToBeHiddenAsync();
        await Expect(yearByYearPanel).ToBeVisibleAsync();

        await Expect(toggleButton).ToBeVisibleAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "true");

        await toggleButton.ClickAsync();

        await Expect(currentYearPanel).ToBeVisibleAsync();
        await Expect(yearByYearPanel).ToBeHiddenAsync();

        await Expect(toggleButton).ToBeVisibleAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "false");
    }

    [Fact]
    public async Task EnglishMaths_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(EnglishMathsHeaderText);
        var topPerfomersTab = section.GetByRole(AriaRole.Tab, new() { Name = "Top performing similar schools" });
        await topPerfomersTab.ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var values = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(values).ToBePercentageValuesHavingCount(3);

        var schools = await table.GetTableColumnAsync("School");
        await Expect(schools).ToHaveCountAsync(3);

        var schoolLinks = schools.GetByRole(AriaRole.Link);
        await schoolLinks.Nth(0).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task EnglishMaths_ViewTableView()
    {
        var section = await GetSection(EnglishMathsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var previous2 = await table.GetTableColumnAsync("2022 to 2023");
        await Expect(previous2).ToBePercentageValuesHavingCount(4);

        var previous = await table.GetTableColumnAsync("2023 to 2024");
        await Expect(previous).ToBePercentageValuesHavingCount(4);

        var current = await table.GetTableColumnAsync("2024 to 2025");
        await Expect(current).ToBePercentageValuesHavingCount(4);
    }

    [Fact]
    public async Task EnglishMaths_ChangeGradeFilters()
    {
        var section = await GetSection(EnglishMathsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await table.GetCells().AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await table.GetCells().AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task Destinations_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(DestinationsHeaderText);
        var panel = section.GetByRole(AriaRole.Tabpanel);

        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearPanel = section.Locator("[data-content-toggle-name=\"2022 to 2023\"]");
        var yearByYearPanel = section.Locator("[data-content-toggle-name=\"Year by year\"]");
        var toggleButton = section.Locator(".app-content-toggle__header button[type=\"button\"]");


        await Expect(currentYearPanel).ToBeVisibleAsync();
        await Expect(yearByYearPanel).ToBeHiddenAsync();

        await Expect(toggleButton).ToBeVisibleAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "false");

        await toggleButton.ClickAsync();

        await Expect(currentYearPanel).ToBeHiddenAsync();
        await Expect(yearByYearPanel).ToBeVisibleAsync();

        await Expect(toggleButton).ToBeVisibleAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "true");

        await toggleButton.ClickAsync();

        await Expect(currentYearPanel).ToBeVisibleAsync();
        await Expect(yearByYearPanel).ToBeHiddenAsync();

        await Expect(toggleButton).ToBeVisibleAsync();
        await Expect(toggleButton).ToHaveAttributeAsync("aria-pressed", "false");
    }

    [Fact]
    public async Task Destinations_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(DestinationsHeaderText);
        var topPerfomersTab = section.GetByRole(AriaRole.Tab, new() { Name = "Top performing similar schools" });
        await topPerfomersTab.ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var values = await table.GetTableColumnAsync("2022 to 2023");
        await Expect(values).ToBePercentageValuesHavingCount(3);

        var schools = await table.GetTableColumnAsync("School");
        await Expect(schools).ToHaveCountAsync(3);

        var schoolLinks = schools.GetByRole(AriaRole.Link);
        await schoolLinks.Nth(0).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task Destinations_ViewTableView()
    {
        var section = await GetSection(DestinationsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        var previous2 = await table.GetTableColumnAsync("2020 to 2021");
        await Expect(previous2).ToBePercentageValuesHavingCount(4);

        var previous = await table.GetTableColumnAsync("2021 to 2022");
        await Expect(previous).ToBePercentageValuesHavingCount(4);

        var current = await table.GetTableColumnAsync("2022 to 2023");
        await Expect(current).ToBePercentageValuesHavingCount(4);
    }

    [Fact]
    public async Task Destinations_ChangeDestinationFilters()
    {
        var section = await GetSection(DestinationsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> subjectValues = [];

        subjectValues.Add(await table.GetCells().AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Education", "Employment and apprenticeships" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Destination" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            subjectValues.Add(await table.GetCells().AllTrimmedTextContentsAsync());
        }

        subjectValues.Should().AllBeDifferent();
    }

    private async Task<ILocator> GetSection(string headerText)
    {
        var section = Page.GetByLabel(headerText);
        await Expect(section).ToBeVisibleAsync();

        return section;
    }
}
