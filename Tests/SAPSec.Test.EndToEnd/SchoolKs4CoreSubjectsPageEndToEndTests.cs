using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Common.FluentAssertions;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using System.Text.RegularExpressions;
using Xunit;

namespace SAPSec.Test.EndToEnd;

[Collection("EndToEndTestsCollection")]
public class SchoolKs4CoreSubjectsPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string UrlPattern = @"\d{6}";
    private const string EnglishLanguageHeaderText = "English language";
    private const string EnglishLiteratureHeaderText = "English literature";
    private const string MathsHeaderText = "Maths";
    private const string CombinedScienceHeaderText = "Combined science (double award)";
    private const string BiologyHeaderText = "Biology";
    private const string ChemistryHeaderText = "Chemistry";
    private const string PhysicsHeaderText = "Physics";

    private const string Urn = "100052";
    private static readonly Routes.Secondary SecondarySchoolRoute = Routes.SecondarySchool(Urn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(Urn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.Overview);
        await Page.GetByText("KS4 core subjects", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.KS4CoreSubjects);
    }

    [Fact]
    public async Task EnglishLanguage_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(EnglishLanguageHeaderText);
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
    public async Task EnglishLanguage_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(EnglishLanguageHeaderText);
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

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task EnglishLanguage_ViewTableView()
    {
        var section = await GetSection(EnglishLanguageHeaderText);
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

    [Fact]
    public async Task EnglishLanguage_ChangeGradeFilters()
    {
        var section = await GetSection(EnglishLanguageHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5 and above", "Grade 7 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task EnglishLiterature_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(EnglishLiteratureHeaderText);
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
    public async Task EnglishLiterature_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(EnglishLiteratureHeaderText);
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

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task EnglishLiterature_ViewTableView()
    {
        var section = await GetSection(EnglishLiteratureHeaderText);
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

    [Fact]
    public async Task EnglishLiterature_ChangeGradeFilters()
    {
        var section = await GetSection(EnglishLiteratureHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5 and above", "Grade 7 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task Maths_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(MathsHeaderText);
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
    public async Task Maths_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(MathsHeaderText);
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

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task Maths_ViewTableView()
    {
        var section = await GetSection(MathsHeaderText);
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

    [Fact]
    public async Task Maths_ChangeGradeFilters()
    {
        var section = await GetSection(MathsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5 and above", "Grade 7 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task CombinedScience_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(CombinedScienceHeaderText);
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
    public async Task CombinedScience_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(CombinedScienceHeaderText);
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

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task CombinedScience_ViewTableView()
    {
        var section = await GetSection(CombinedScienceHeaderText);
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

    [Fact]
    public async Task CombinedScience_ChangeGradeFilters()
    {
        var section = await GetSection(CombinedScienceHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5-5 and above", "Grade 7-7 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task Biology_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(BiologyHeaderText);
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
    public async Task Biology_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(BiologyHeaderText);
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

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task Biology_ViewTableView()
    {
        var section = await GetSection(BiologyHeaderText);
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

    [Fact]
    public async Task Biology_ChangeGradeFilters()
    {
        var section = await GetSection(BiologyHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5 and above", "Grade 7 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task Chemistry_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(ChemistryHeaderText);
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
    public async Task Chemistry_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(ChemistryHeaderText);
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

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task Chemistry_ViewTableView()
    {
        var section = await GetSection(ChemistryHeaderText);
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

    [Fact]
    public async Task Chemistry_ChangeGradeFilters()
    {
        var section = await GetSection(ChemistryHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5 and above", "Grade 7 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    [Fact]
    public async Task Physics_ToggleBetweenYearByYearAndCurrentYearView()
    {
        var section = await GetSection(PhysicsHeaderText);
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
    public async Task Physics_ViewAndNavigateToTopPerfomers()
    {
        var section = await GetSection(PhysicsHeaderText);
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

        await Expect(Page).ToHaveURLAsync(new Regex(SecondarySchoolRoute.Comparison(UrlPattern).Similarity));

        await Page.GoBackAsync();

        await Expect(section).ToBeVisibleAsync();
        await Expect(topPerfomersTab).ToBeVisibleAsync();
        await topPerfomersTab.ClickAsync();

        await section.GetByText("See all similar schools").ClickAsync();

        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.ViewSimilarSchools);
    }

    [Fact]
    public async Task Physics_ViewTableView()
    {
        var section = await GetSection(PhysicsHeaderText);
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

    [Fact]
    public async Task Physics_ChangeGradeFilters()
    {
        var section = await GetSection(PhysicsHeaderText);
        await section.GetByRole(AriaRole.Tab, new() { Name = "Table" }).ClickAsync();

        var table = section.GetByRole(AriaRole.Table);
        await Expect(table).ToBeVisibleAsync();

        List<IEnumerable<string>> gradeValues = [];

        gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());

        foreach (var subject in new[] { "Grade 5 and above", "Grade 7 and above" })
        {
            await section.GetByRole(AriaRole.Combobox, new() { Name = "Grade" }).SelectOptionAsync(subject);
            await table.WaitForDomToStopChanging();

            gradeValues.Add(await (table.GetCells()).AllTrimmedTextContentsAsync());
        }

        gradeValues.Should().AllBeDifferent();
    }

    private async Task<ILocator> GetSection(string headerText)
    {
        var section = Page.GetByLabel(headerText);
        await Expect(section).ToBeVisibleAsync();

        return section;
    }
}
