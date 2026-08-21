using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd;

[Collection("EndToEndTestsCollection")]
public class ComparisonKs4HeadlineMeasuresPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string UrlPattern = @"\d{6}";
    private const string Attainment8HeaderText = "Attainment 8";
    private const string EnglishMathsHeaderText = "Grade achieved in English and maths GCSEs";
    private const string DestinationsHeaderText = "Staying in education or entering employment";

    private const string Urn = "100052";
    private const string SimilarSchoolUrn = "141617";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(Urn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(Urn).Overview);
        await Page.GetByText("View similar schools", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(Urn).ViewSimilarSchools);
        await Page.GetByText("The Hurlingham Academy", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(Urn).Comparison(SimilarSchoolUrn).Overview);
        await Page.GetByText("KS4 headline measures", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(Urn).Comparison(SimilarSchoolUrn).KS4HeadlineMeasures);
    }

    [Fact]
    public async Task Attainment8_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(Attainment8HeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2024 to 2025" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });

        await Expect(currentYearHeader).ToBeVisibleAsync();
        await Expect(yearByYearHeader).ToBeHiddenAsync();

        await section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" }).ClickAsync();

        await Expect(currentYearHeader).ToBeHiddenAsync();
        await Expect(yearByYearHeader).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Attainment8_ViewTableView()
    {
        var section = await GetSection(Attainment8HeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

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
    public async Task EnglishMaths_ViewTableView()
    {
        var section = await GetSection(EnglishMathsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

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
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task Destinations_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(DestinationsHeaderText);
        var panel = section.GetByRole(AriaRole.Tabpanel);

        await section.GetByRole(AriaRole.Tab, new() { Name = "Charts" }).ClickAsync();

        var currentYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "2022 to 2023" });
        var yearByYearHeader = section.GetByRole(AriaRole.Heading, new() { Name = "Year by year" });

        var showYearByYearButton = section.GetByRole(AriaRole.Button, new() { Name = "Show year by year" });
        var showCurrentYearButton = section.GetByRole(AriaRole.Button, new() { Name = "Show 2022 to 2023" });

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
    public async Task Destinations_ViewTableView()
    {
        var section = await GetSection(DestinationsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

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
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> subjectValues = [];

        subjectValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Education", "Employment and apprenticeships" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Destination" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            subjectValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
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
