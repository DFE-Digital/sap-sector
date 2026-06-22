using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration;

[Collection("IntegrationTestsCollection")]
public class FindASchoolPageIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task SearchResultsPage_ViaSubmittingSearchForm_ShouldShowCorrectResultCount()
    {
        var page = await fixture.RequestPageAsync(Routes.FindASchool(), HttpStatusCode.OK);

        var searchInput = page.InputWithNameShouldExist("Query");
        searchInput.Value = "school";

        var searchButton = page.ButtonWithNameShouldExist("Search");
        var resultsPage = await page.SubmitContainingFormAsync(searchButton, HttpStatusCode.OK);
        outputHelper.WriteLine(resultsPage.Body!.OuterHtml);

        var results = resultsPage.ElementShouldExist(".results-bar .app-school-results-count");
        results.TrimmedTextContent().Should().Be("Showing 1 - 10 of 833 schools");
    }

    [Fact]
    public async Task SearchResultsPage_ViaQueryString_ShouldShowCorrectResultCount()
    {
        var resultsPage = await fixture.RequestPageAsync(Routes.FindASchool(query: "school"), HttpStatusCode.OK);

        var results = resultsPage.ElementShouldExist(".results-bar .app-school-results-count");
        results.TrimmedTextContent().Should().Be("Showing 1 - 10 of 833 schools");
    }
}
