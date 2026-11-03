using FluentAssertions;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class SchoolPagesIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task OverviewPage_ContainsWhatIsASimilarSchoolLink()
    {
        //Fixture.EstablishmentRepository.SetupEstablishments(
        //    Build.Establishment("100001", "Test School 1", x => x.Open().Primary()));

    
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        Fixture.SimilarSchoolsPrimaryRepository.SetupValues(
            Build.PrimaryValues(["100001", "100002"]));

            var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Overview);

        var link = page.QuerySelector(".app-body-container-with-side-navigation a");
        link.Should().NotBeNull();
        link.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").WhatIsASimilarSchool);
    }

    [Fact]
    public async Task OverviewPage_ShowsGenericContent()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        Fixture.SimilarSchoolsPrimaryRepository.SetupValues(
            Build.PrimaryValues(["100001", "100002"]));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Overview);

        page.QuerySelector(".app-overview__lead")!.TextContent.Trim()
            .Should().Be("For primary phase schools we identify 50 similar primary phase schools, including all-throughs, to help you:");

        var lists = page.QuerySelectorAll(".app-overview__list");
        lists.First().QuerySelectorAll("li").Select(x => x.TextContent.Trim())
            .Should().Equal(
                "compare performance data",
                "find improvement opportunities",
                "connect with school leaders and share insights");
        lists.Skip(1).First().QuerySelectorAll("li").Select(x => x.TextContent.Trim())
            .Should().Equal(
                "similar school averages",
                "the local authority average",
                "the national average");

        var text = page.QuerySelector(".app-overview")!.TextContent;
        text.Should().Contain("We use 9 characteristics to find similar schools.");
        text.Should().Contain("Review the full list of schools this school has matched with individually");
        text.Should().NotContain("similar to yours").And.NotContain("you've matched");

        var link = page.QuerySelector(".app-overview a")!;
        link.TextContent.Trim().Should().Be("how DfE identifies what a similar school is");
        link.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").WhatIsASimilarSchool);
        link.GetAttribute("target").Should().BeNull();

        page.QuerySelectorAll(".app-overview h2").Select(x => x.TextContent.Trim())
            .Should().Equal("Compare school performance", "View similar schools to connect with");
    }

    [Fact]
    public async Task WhatIsASimilarSchoolPage_ContainsViewSimilarSchoolsLink()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary()));

        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").WhatIsASimilarSchool);

        var links = page.QuerySelectorAll(".app-body-container-with-side-navigation a");
        links.Should().Contain(l => l.GetAttribute("href") == Routes.PrimarySchool("100001").ViewSimilarSchools);
    }
}
