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

}