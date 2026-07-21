using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class Ks2PerformanceMeasuresJsonIntegrationTests(
    JsonRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : JsonRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task Ks2Page_ForSampleJsonSchool_ShouldRenderReadingScoreMeasure()
    {
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100134").KS2, HttpStatusCode.OK);

        page.QuerySelector("h1")!.TextContent.Trim().Should().Be("KS2 performance measures");
        page.ElementWithTestIdShouldExist("reading-score-heading");
        page.ElementWithTestIdShouldExist("reading-score-table-view-table");
    }
}
