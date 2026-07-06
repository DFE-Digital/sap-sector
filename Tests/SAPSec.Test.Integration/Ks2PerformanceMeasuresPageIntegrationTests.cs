using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration;

[Collection("InMemoryStoreIntegrationTestsCollection")]
public class Ks2PerformanceMeasuresPageIntegrationTests(InMemoryStoreIntegrationTestFixture fixture, ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task TableView_ShouldShowCorrectValues()
    {
        fixture.EstablishmentStore.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        fixture.SimilarSchoolsPrimaryStore.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        fixture.Ks2PerformanceStore.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected("101", "100", "99")));

        fixture.Ks2PerformanceStore.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected("91", "90", "89")));

        fixture.Ks2PerformanceStore.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected("81", "80", "79")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected("71", "70", "69")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected("71", "70", "69")));

        await fixture.RebuildSearchIndex();

        var page = await fixture.RequestPageAsync(Routes.PrimarySchool("100001").Ks2, HttpStatusCode.OK);

        outputHelper.WriteLine(page.Body!.OuterHtml);

        var table = page.TableShouldExist("#expected-rwm-table-view table");

        table.ShouldHaveRows(
            ["School(s)", "2022 to 2023", "2023 to 2024", "2024 to 2025", "3-year average"],
            ["Test School 1", "79.0", "80.0", "81.0", "80.0"],
            ["Similar schools average", "69.0", "70.0", "71.0", "70.0"],
            ["Local authority schools average", "89.0", "90.0", "91.0", "90.0"],
            ["Schools in England average", "99.0", "100.0", "101.0", "100.0"]);
    }
}