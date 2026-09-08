using FluentAssertions;
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
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Overview);

        var link = page.QuerySelector(".app-body-container-with-side-navigation a");
        link.Should().NotBeNull();
        link.GetAttribute("href").Should().Be(Routes.PrimarySchool("100001").WhatIsASimilarSchool);
    }

    [Fact]
    public async Task WhatIsASimilarSchoolPage_ContainsViewSimilarSchoolsLink()
    {
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").WhatIsASimilarSchool);

        var links = page.QuerySelectorAll(".app-body-container-with-side-navigation a");
        links.Should().Contain(l => l.GetAttribute("href") == Routes.PrimarySchool("100001").ViewSimilarSchools);
    }
}
