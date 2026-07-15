using FluentAssertions;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class ViewSimilarSchoolsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task ViewSimilarSchools_ShowsCurrentSchoolSummaryAndSimilarSchoolsTable()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001", "Test LA 1")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002", "Test LA 2")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003", "Test LA 3")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Rank = "1", Dist = "0.1" },
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Rank = "2", Dist = "0.2" });

        Fixture.SimilarSchoolsPrimaryRepository.SetupValues(
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100001",
                PPPerc = "20.2",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEhp = "1.5",
                NumberOfPupils = "210",
                ReadMatAverage = "102.4",
                Ks1PriorRwmAverage = "11.4"
            },
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100002",
                PPPerc = "25.4",
                Polar4QuintilePupils = "3",
                PStability = "94",
                PercentSchSupport = "12",
                PercentEAL = "6",
                IdaciPupils = "0.223",
                PercentageStatementOrEhp = "2.5",
                NumberOfPupils = "220",
                ReadMatAverage = "101.4",
                Ks1PriorRwmAverage = "10.4"
            },
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100003",
                PPPerc = "30.5",
                Polar4QuintilePupils = "4",
                PStability = "93",
                PercentSchSupport = "14",
                PercentEAL = "7",
                IdaciPupils = "0.323",
                PercentageStatementOrEhp = "3.5",
                NumberOfPupils = "230",
                ReadMatAverage = "100.4",
                Ks1PriorRwmAverage = "9.4"
            });

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").ViewSimilarSchools, HttpStatusCode.OK);

        var summary = page.ElementWithTestIdShouldExist("primary-current-school-summary");
        summary.TextContent.Should().Contain("102.4");
        summary.TextContent.Should().Contain("11.4");
        summary.TextContent.Should().Contain("20.2%");

        var list = page.ElementWithTestIdShouldExist("primary-similar-schools-list");
        list.TextContent.Should().Contain("Test School 2");
        list.TextContent.Should().Contain("Test LA 2");
        list.TextContent.Should().Contain("Rank: 1");
        list.TextContent.Should().Contain("Distance: 0.1");
        list.TextContent.Should().Contain("101.4");
        list.TextContent.Should().Contain("10.4");
        list.TextContent.Should().Contain("220");
        list.TextContent.Should().Contain("25.4%");
        list.TextContent.Should().Contain("6%");
        list.TextContent.Should().Contain("Test School 3");
        list.TextContent.Should().Contain("Test LA 3");
        list.TextContent.Should().Contain("Rank: 2");
        list.TextContent.Should().Contain("Distance: 0.2");

        var links = list.QuerySelectorAll("a").Select(x => x.GetAttribute("href"));
        links.Should().BeEquivalentTo([
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100002"),
            Routes.PrimarySchool("100001").SimilarSchoolComparison("100003")
        ]);
    }
}
