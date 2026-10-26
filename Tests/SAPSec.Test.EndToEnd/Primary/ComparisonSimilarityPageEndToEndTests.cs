using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Primary;

[Collection("EndToEndTestsCollection")]
public class ComparisonSimilarityPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string CurrentSchoolUrn = "101206";
    private const string CurrentSchoolName = "Grafton Primary School";
    // ComparatorSchoolUrn is a confirmed-good pairing (both schools have characteristics +
    // KS2 data). The similar-schools list defaults to sorting by RwmExpected performance
    // value (descending), not similarity rank, and is paginated at 10 per page - this
    // school sits at position 34 of 50 in that ordering, which lands it on page 4.
    private const string ComparatorSchoolUrn = "101230";
    private const string ComparatorSchoolName = "Roding Primary School";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Compare and connect with similar schools", new() { Exact = true }).FillAsync(CurrentSchoolUrn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.PrimarySchool(CurrentSchoolUrn).Overview);
        await Page.GetByRole(AriaRole.Link, new() { Name = "View similar schools", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.PrimarySchool(CurrentSchoolUrn).ViewSimilarSchools);
        await NavigateTo($"{Routes.PrimarySchool(CurrentSchoolUrn).ViewSimilarSchools}?page=4");
        await Page.GetByRole(AriaRole.Link, new() { Name = ComparatorSchoolName, Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.PrimarySchool(CurrentSchoolUrn).Comparison(ComparatorSchoolUrn).Similarity);
    }

    [Fact]
    public async Task SimilarSchoolComparison_CanNavigateToSimilarity_AndSeeCharacteristicsTable()
    {
        var heading = Page.Locator("h2.govuk-heading-l");
        await heading.WaitForAsync();
        (await heading.TextContentAsync()).Should().Contain("How these schools compare");

        var table = Page.Locator("table.govuk-table");
        await table.WaitForAsync();
        (await table.CountAsync()).Should().Be(1, "Similarity table should be visible");

        var rows = table.Locator("tbody tr.govuk-table__row");
        (await rows.CountAsync()).Should().Be(9, "Similarity table should list 9 characteristics");
    }
}