using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration;

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

        page.ElementWithTextContentShouldExist("h2", "Progress score in reading, writing and maths");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_MeasureExistsOnPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        page.ElementWithTextContentShouldExist("h2", "Meeting expected standard in reading, writing and maths");
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

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "101", prev: "100", prev2: "99")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "91", prev: "90", prev2: "89")));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.TableShouldExist("#expected-rwm-table-view table");

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

        Fixture.Ks2PerformanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "101.31", prev: "99.52", prev2: "99.49")));

        Fixture.Ks2PerformanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "91.02", prev: "89.7", prev2: "89.1")));

        Fixture.Ks2PerformanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "80.99", prev: "80.3", prev2: "78.9")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "70.6", prev: "70.3", prev2: "69.1")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "71.1", prev: "70.2", prev2: "69.3")));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2, HttpStatusCode.OK);

        var table = page.TableShouldExist("#expected-rwm-table-view table");

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

        var table = page.TableShouldExist("#expected-rwm-top-performers table");

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

        OutputHelper.WriteLine(page.DocumentElement.OuterHtml);

        var similarSchoolsLink = page.LinkWithTextContentShouldExist("See all similar schools");
        similarSchoolsLink.RelativeHref().Should().Be(Routes.PrimarySchool("100001").ViewSimilarSchools);

        var topPerfomersLinks = page.QuerySelectorAll("#expected-rwm-top-performers table a");
        var hrefs = topPerfomersLinks.Should().AllBeAssignableTo<IHtmlAnchorElement>()
            .Subject.Select(l => l.RelativeHref());

        hrefs.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100004"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002")
        ]);
    }
}