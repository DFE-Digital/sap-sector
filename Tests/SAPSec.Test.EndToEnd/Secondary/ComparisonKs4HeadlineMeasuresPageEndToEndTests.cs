using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Secondary;

[Collection("EndToEndTestsCollection")]
public class ComparisonKs4HeadlineMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string UrlPattern = @"\d{6}";
    private const string Attainment8HeaderText = "Attainment 8";
    private const string EnglishMathsHeaderText = "Grade achieved in English and maths GCSEs";
    private const string DestinationsHeaderText = "Staying in education or entering employment";

    private const string CurrentSchoolUrn = "100052";
    private const string CurrentSchoolName = "Hampstead School";
    private const string ComparatorSchoolUrn = "141617";
    private const string ComparatorSchoolName = "The Hurlingham Academy";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(CurrentSchoolUrn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Overview);
        await Page.GetByText("View similar schools", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).ViewSimilarSchools);
        await Page.GetByText(ComparatorSchoolName, new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Comparison(ComparatorSchoolUrn).Similarity);
        await Page.GetByText("KS4 headline measures", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Comparison(ComparatorSchoolUrn).KS4HeadlineMeasures);
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
    public async Task FooterAccessibilityLink_NavigatesToManagedAccessibilityStatementInSameTab()
    {
        await Page.RouteAsync($"{LayoutConstants.AccessibilityStatementUrl}*", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "text/html",
                Body = "<html><head><title>Accessibility statement</title></head><body>Accessibility statement</body></html>"
            });
        });

        var accessibilityLink = Page.Locator("footer").GetByRole(AriaRole.Link, new() { Name = "Accessibility" });

        await Expect(accessibilityLink).ToBeVisibleAsync();
        (await accessibilityLink.GetAttributeAsync("href")).Should().Be(LayoutConstants.AccessibilityStatementUrl);
        (await accessibilityLink.GetAttributeAsync("target")).Should().BeNull();

        await accessibilityLink.ClickAsync();
        await Page.WaitForURLAsync($"{LayoutConstants.AccessibilityStatementUrl}*");

        Page.Url.Should().Be(LayoutConstants.AccessibilityStatementUrl);
        new Uri(Page.Url).AbsolutePath.Should().NotBe("/accessibility");
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
            await Expect(values).ToBeNumericValuesHavingCount(3);
        }
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
    public async Task EnglishMaths_ViewTableView()
    {
        var section = await GetSection(EnglishMathsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        foreach (var heading in new[] { "2022 to 2023", "2023 to 2024", "2024 to 2025" })
        {
            var values = await table.GetTableColumnAsync(heading);
            await Expect(values).ToBePercentageValuesHavingCount(3);
        }
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
    public async Task Destinations_ViewTableView()
    {
        var section = await GetSection(DestinationsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table of data" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        foreach (var heading in new[] { "2020 to 2021", "2021 to 2022", "2022 to 2023" })
        {
            var values = await table.GetTableColumnAsync(heading);
            await Expect(values).ToBePercentageValuesHavingCount(3);
        }
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
