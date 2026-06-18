using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;

namespace SAPSec.Test.Integration;

[Collection("IntegrationTestsCollection")]
public class FindASchoolPageIntegrationTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task SearchPage()
    {
        var page = await fixture.RequestPageAsync(Routes.FindASchool(), HttpStatusCode.OK);

        var nav = page.ElementShouldExist("#navigation");
        nav.ChildTrimmedTextContent().Should().Equal(["Find a school", "Sign out"]);
    }

    [Fact]
    public async Task SearchResultsPage_ViaSubmittingSearchForm_ShouldShowCorrectResultCount()
    {
        fixture.EstablishmentStore.SetupEstablishments(
            new Data.Dto.Establishment
            {
                URN = "100001",
                EstablishmentName = "Test School 1",
                PhaseOfEducationName = "Secondary"
            },
            new Data.Dto.Establishment
            {
                URN = "100002",
                EstablishmentName = "Test School 2",
                PhaseOfEducationName = "Secondary"
            });

        await fixture.RebuildSearchIndex();

        var page = await fixture.RequestPageAsync(Routes.FindASchool(), HttpStatusCode.OK);

        var searchInput = page.InputWithNameShouldExist("Query");
        searchInput.Value = "Test";

        var searchButton = page.ButtonWithNameShouldExist("Search");
        var resultsPage = await page.SubmitContainingFormAsync(searchButton, HttpStatusCode.OK);

        var results = resultsPage.ElementShouldExist(".results-bar .app-school-results-count");
        results.TrimmedTextContent().Should().Be("Showing 1 - 2 of 2 schools");
    }

    [Fact]
    public async Task SearchResultsPage_ViaQueryString_ShouldShowCorrectResultCount()
    {
        fixture.EstablishmentStore.SetupEstablishments(
            new Data.Dto.Establishment
            {
                URN = "100001",
                EstablishmentName = "Another School 1",
                PhaseOfEducationName = "Secondary"
            },
            new Data.Dto.Establishment
            {
                URN = "100002",
                EstablishmentName = "Another School 2",
                PhaseOfEducationName = "Secondary"
            });

        await fixture.RebuildSearchIndex();

        var resultsPage = await fixture.RequestPageAsync(Routes.FindASchool(query: "another"), HttpStatusCode.OK);

        var results = resultsPage.ElementShouldExist(".results-bar .app-school-results-count");
        results.TrimmedTextContent().Should().Be("Showing 1 - 2 of 2 schools");
    }
}
