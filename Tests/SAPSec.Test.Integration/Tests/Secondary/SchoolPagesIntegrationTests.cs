using FluentAssertions;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

public class SchoolPagesIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task OverviewPage_ContainsWhatIsASimilarSchoolLink()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").Overview);

        var link = page.QuerySelector(".app-body-container-with-side-navigation a");
        link.Should().NotBeNull();
        link.GetAttribute("href").Should().Be(Routes.SecondarySchool("100001").WhatIsASimilarSchool);
    }

    [Fact]
    public async Task WhatIsASimilarSchoolPage_ContainsViewSimilarSchoolsLink()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").WhatIsASimilarSchool);

        var links = page.QuerySelectorAll(".app-body-container-with-side-navigation a");
        links.Should().Contain(l => l.GetAttribute("href") == Routes.SecondarySchool("100001").ViewSimilarSchools);
    }
}
