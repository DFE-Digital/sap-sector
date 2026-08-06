using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class Ks2PerformanceMeasuresPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task Progress8_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("progress-rwm-heading");
        heading.TrimmedTextContent().Should().Be("Progress score in reading, writing and maths");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("expected-rwm-heading");
        heading.TrimmedTextContent().Should().Be("Meeting expected standard in reading, writing and maths");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "101", prev: "100", prev2: "99")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "91", prev: "90", prev2: "89")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "70.6", prev: "70.3", prev2: "69.1")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71.1", prev: "70.2", prev2: "69.3")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "101.31", prev: "99.52", prev2: "99.49")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "91.02", prev: "89.7", prev2: "89.1")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmExpected(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 4", "22%"],
            ["2", "Test School 3", "21%"],
            ["3", "Test School 2", "20%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmExpected(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("expected-rwm-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-top-performers-table");
        var topPerfomersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerfomersLinks.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100004"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002")
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SubjectFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("expected-rwm-subject-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Reading, writing and maths", "Reading", "Writing", "Maths"]);
    }

    [InlineData("Reading", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "71%", "72%", "73%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Writing", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "61%", "62%", "63%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Maths", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "51%", "52%", "53%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task MeetingExpectedStandardRwm_SubjectFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] la, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmExpectedReading(current: "72", prev: "71", prev2: "70")
                .WithRwmExpectedWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmExpectedMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmExpectedReading(current: "72", prev: "71", prev2: "70")
                .WithRwmExpectedWriting(current: "60", prev: "59", prev2: "58")
                .WithRwmExpectedMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmExpectedReading(current: "70", prev: "69", prev2: "68")
                .WithRwmExpectedWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmExpectedMaths(current: "50", prev: "49", prev2: "48")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
             Build.Ks2Performance.LA("001", x => x
                .WithRwmExpectedReading(current: "73", prev: "72", prev2: "71")
                .WithRwmExpectedWriting(current: "63", prev: "62", prev2: "61")
                .WithRwmExpectedMaths(current: "53", prev: "52", prev2: "51")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmExpectedReading(current: "74", prev: "73", prev2: "72")
                .WithRwmExpectedWriting(current: "64", prev: "63", prev2: "62")
                .WithRwmExpectedMaths(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("expected-rwm-subject-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("expected-rwm-subject-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Similar schools average", .. similarSchools],
            ["Local authority schools average", .. la],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("higher-rwm-heading");
        heading.TrimmedTextContent().Should().Be("Achieved a higher standard in reading, writing and maths");
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "71", prev: "70", prev2: "69")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "71", prev: "70", prev2: "69")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmHigher(current: "101", prev: "100", prev2: "99")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmHigher(current: "91", prev: "90", prev2: "89")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TableView_ValuesRoundTo0DecimalPlaces()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "70.6", prev: "70.3", prev2: "69.1")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "71.1", prev: "70.2", prev2: "69.3")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmHigher(current: "101.31", prev: "99.52", prev2: "99.49")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmHigher(current: "91.02", prev: "89.7", prev2: "89.1")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "79%", "80%", "81%"],
            ["Similar schools average", "69%", "70%", "71%"],
            ["Local authority schools average", "89%", "90%", "91%"],
            ["Schools in England average", "99%", "100%", "101%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmHigher(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmHigher(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 4", "22%"],
            ["2", "Test School 3", "21%"],
            ["3", "Test School 2", "20%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmHigher(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmHigher(current: "19", prev: "61", prev2: "67")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("higher-rwm-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-top-performers-table");
        var topPerfomersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerfomersLinks.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100004"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002")
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_SubjectFilter_HasExpectedOptions()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist("higher-rwm-subject-filter");
        filter.ChildTrimmedTextContent().Should().Equal(["Reading, writing and maths", "Reading", "Writing", "Maths"]);
    }

    [InlineData("Reading", new[] { "70%", "71%", "72%" }, new[] { "69%", "70%", "71%" }, new[] { "71%", "72%", "73%" }, new[] { "72%", "73%", "74%" })]
    [InlineData("Writing", new[] { "60%", "61%", "62%" }, new[] { "59%", "60%", "61%" }, new[] { "61%", "62%", "63%" }, new[] { "62%", "63%", "64%" })]
    [InlineData("Maths", new[] { "50%", "51%", "52%" }, new[] { "49%", "50%", "51%" }, new[] { "51%", "52%", "53%" }, new[] { "52%", "53%", "54%" })]
    [Theory]
    public async Task AchievedHigherStandardRwm_SubjectFilter_UpdatesTableViewWithSubjectValues(string filterOption, string[] currentSchool, string[] similarSchools, string[] la, string[] england)
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmHigherReading(current: "72", prev: "71", prev2: "70")
                .WithRwmHigherWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmHigherMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmHigherReading(current: "72", prev: "71", prev2: "70")
                .WithRwmHigherWriting(current: "60", prev: "59", prev2: "58")
                .WithRwmHigherMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmHigherReading(current: "70", prev: "69", prev2: "68")
                .WithRwmHigherWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmHigherMaths(current: "50", prev: "49", prev2: "48")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
             Build.Ks2Performance.LA("001", x => x
                .WithRwmHigherReading(current: "73", prev: "72", prev2: "71")
                .WithRwmHigherWriting(current: "63", prev: "62", prev2: "61")
                .WithRwmHigherMaths(current: "53", prev: "52", prev2: "51")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmHigherReading(current: "74", prev: "73", prev2: "72")
                .WithRwmHigherWriting(current: "64", prev: "63", prev2: "62")
                .WithRwmHigherMaths(current: "54", prev: "53", prev2: "52")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var filter = page.ElementWithTestIdShouldExist<IHtmlSelectElement>("higher-rwm-subject-filter");
        filter.SelectOption(filterOption);

        var submitButton = page.ElementWithTestIdShouldExist<IHtmlButtonElement>("higher-rwm-subject-filter-submit");
        var newPage = await page.SubmitContainingFormAsync(submitButton);

        var table = newPage.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-rwm-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", .. currentSchool],
            ["Similar schools average", .. similarSchools],
            ["Local authority schools average", .. la],
            ["Schools in England average", .. england]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("reading-score-heading");
        heading.TrimmedTextContent().Should().Be("Average scaled score in reading");
    }

    [Fact]
    public async Task AverageScaledScoreReading_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithReadingScaledScore(current: "107.4", prev: "106.6", prev2: "105.8")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithReadingScaledScore(current: "104.5", prev: "103.5", prev2: "102.5")));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.4", prev: "100.4", prev2: "99.4")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "103.2", prev: "102.2", prev2: "101.2")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "105.2", prev: "104.2", prev2: "103.2")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("reading-score-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "99.4", "100.4", "101.4"],
            ["Similar schools average", "102.2", "103.2", "104.2"],
            ["Local authority schools average", "102.5", "103.5", "104.5"],
            ["Schools in England average", "105.8", "106.6", "107.4"]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "104.2", prev: "102.8", prev2: "101.8")),
            Build.Ks2Performance.Establishment("100004", x => x.WithReadingScaledScore(current: "106.3", prev: "105.4", prev2: "104.4")),
            Build.Ks2Performance.Establishment("100005", x => x.WithReadingScaledScore(current: "103.7", prev: "102.9", prev2: "101.9")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("reading-score-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 4", "106.3"],
            ["2", "Test School 2", "104.2"],
            ["3", "Test School 3", "104.2"]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "104.2", prev: "102.8", prev2: "101.8")),
            Build.Ks2Performance.Establishment("100004", x => x.WithReadingScaledScore(current: "106.3", prev: "105.4", prev2: "104.4")),
            Build.Ks2Performance.Establishment("100005", x => x.WithReadingScaledScore(current: "103.7", prev: "102.9", prev2: "101.9")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("reading-score-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("reading-score-top-performers-table");
        var topPerformersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerformersLinks.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100004"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003")
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("maths-score-heading");
        heading.TrimmedTextContent().Should().Be("Average scaled score in maths");
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithMathsScaledScore(current: "108.4", prev: "107.6", prev2: "106.8")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithMathsScaledScore(current: "105.5", prev: "104.5", prev2: "103.5")));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.4", prev: "101.4", prev2: "100.4")),
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "104.2", prev: "103.2", prev2: "102.2")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "106.2", prev: "105.2", prev2: "104.2")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("maths-score-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "100.4", "101.4", "102.4"],
            ["Similar schools average", "103.2", "104.2", "105.2"],
            ["Local authority schools average", "103.5", "104.5", "105.5"],
            ["Schools in England average", "106.8", "107.6", "108.4"]);
    }

    [Fact]
    public async Task AverageScaledScoreCharts_ShouldStartAxisAt80()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithReadingScaledScore(current: "108.4", prev: "107.6", prev2: "106.8")
                .WithMathsScaledScore(current: "108.4", prev: "107.6", prev2: "106.8")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x
                .WithReadingScaledScore(current: "105.5", prev: "104.5", prev2: "103.5")
                .WithMathsScaledScore(current: "105.5", prev: "104.5", prev2: "103.5")));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithReadingScaledScore(current: "102.4", prev: "101.4", prev2: "100.4")
                .WithMathsScaledScore(current: "102.4", prev: "101.4", prev2: "100.4")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithReadingScaledScore(current: "104.2", prev: "103.2", prev2: "102.2")
                .WithMathsScaledScore(current: "104.2", prev: "103.2", prev2: "102.2")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithReadingScaledScore(current: "106.2", prev: "105.2", prev2: "104.2")
                .WithMathsScaledScore(current: "106.2", prev: "105.2", prev2: "104.2")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        foreach (var chartId in new[] { "reading-score-school-chart", "maths-score-school-chart" })
        {
            var chart = page.QuerySelector($"#{chartId}");
            chart.Should().NotBeNull();
            chart.GetAttribute("data-axis-min").Should().Be("80");
            chart.GetAttribute("data-axis-step").Should().Be("5");
            chart.GetAttribute("data-axis-max").Should().Be("120");
        }
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.1", prev: "101.5", prev2: "100.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "105.2", prev: "104.1", prev2: "103.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "105.2", prev: "103.8", prev2: "102.8")),
            Build.Ks2Performance.Establishment("100004", x => x.WithMathsScaledScore(current: "107.3", prev: "106.4", prev2: "105.4")),
            Build.Ks2Performance.Establishment("100005", x => x.WithMathsScaledScore(current: "104.7", prev: "103.9", prev2: "102.9")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("maths-score-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 4", "107.3"],
            ["2", "Test School 2", "105.2"],
            ["3", "Test School 3", "105.2"]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.1", prev: "101.5", prev2: "100.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "105.2", prev: "104.1", prev2: "103.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "105.2", prev: "103.8", prev2: "102.8")),
            Build.Ks2Performance.Establishment("100004", x => x.WithMathsScaledScore(current: "107.3", prev: "106.4", prev2: "105.4")),
            Build.Ks2Performance.Establishment("100005", x => x.WithMathsScaledScore(current: "104.7", prev: "103.9", prev2: "102.9")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("maths-score-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("maths-score-top-performers-table");
        var topPerformersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerformersLinks.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100004"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003")
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("expected-gps-heading");
        heading.TrimmedTextContent().Should().Be("Meeting expected standard in grammar, punctuation and spelling");
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "77", prev: "76", prev2: "75")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "76", prev: "75", prev2: "74")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsExpected(current: "69", prev: "68", prev2: "67")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsExpected(current: "73", prev: "72", prev2: "71")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-gps-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "60%", "61%", "62%"],
            ["Similar schools average", "75%", "76%", "77%"],
            ["Local authority schools average", "71%", "72%", "73%"],
            ["Schools in England average", "67%", "68%", "69%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "77", prev: "76", prev2: "75")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "77", prev: "75", prev2: "74")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsExpected(current: "76", prev: "74", prev2: "73")),
            Build.Ks2Performance.Establishment("100005", x => x.WithGpsExpected(current: "70", prev: "69", prev2: "68")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-gps-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Test School 2", "77%"],
            ["2", "Test School 3", "77%"],
            ["3", "Test School 4", "76%"]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "77", prev: "76", prev2: "75")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "77", prev: "75", prev2: "74")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsExpected(current: "76", prev: "74", prev2: "73")),
            Build.Ks2Performance.Establishment("100005", x => x.WithGpsExpected(current: "70", prev: "69", prev2: "68")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("expected-gps-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("expected-gps-top-performers-table");
        var topPerfomersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerfomersLinks.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100004")
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var heading = page.ElementWithTestIdShouldExist("higher-gps-heading");
        heading.TrimmedTextContent().Should().Be("Achieved a higher standard in grammar, punctuation and spelling");
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TableView_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "24", prev: "23", prev2: "22")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "23", prev: "22", prev2: "21")));

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsHigher(current: "15", prev: "14", prev2: "13")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsHigher(current: "19", prev: "18", prev2: "17")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-gps-table-view-table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025"],
            ["Test School 1", "16%", "17%", "18%"],
            ["Similar schools average", "22%", "23%", "24%"],
            ["Local authority schools average", "17%", "18%", "19%"],
            ["Schools in England average", "13%", "14%", "15%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerformers_ShouldShowCorrectValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Thoresby Primary School", x => x.Primary()),
            Build.Establishment("100003", "Manor Park Primary Academy", x => x.Primary()),
            Build.Establishment("100004", "Montem Academy", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "96.6", prev: "23", prev2: "22")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "96.5", prev: "22", prev2: "21")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsHigher(current: "91.4", prev: "21", prev2: "20")),
            Build.Ks2Performance.Establishment("100005", x => x.WithGpsHigher(current: "19", prev: "18", prev2: "17")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-gps-top-performers-table");

        table.ShouldHaveRows(
            ["Rank", "School", "2024 to 2025"],
            ["1", "Manor Park Primary Academy", "97%"],
            ["2", "Thoresby Primary School", "97%"],
            ["3", "Montem Academy", "91%"]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerformers_ShouldLinkToSimilarSchoolsPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "24", prev: "23", prev2: "22")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "24", prev: "22", prev2: "21")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsHigher(current: "23", prev: "21", prev2: "20")),
            Build.Ks2Performance.Establishment("100005", x => x.WithGpsHigher(current: "19", prev: "18", prev2: "17")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var similarSchoolsLink = page.ElementWithTestIdShouldExist("higher-gps-top-performers-similar-schools-link");
        similarSchoolsLink.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").ViewSimilarSchools);

        var table = page.ElementWithTestIdShouldExist<IHtmlTableElement>("higher-gps-top-performers-table");
        var topPerfomersLinks = table.QuerySelectorAll("a")
            .Select(l => l.GetAttribute("href"));

        topPerfomersLinks.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100004")
        ]);
    }

}
