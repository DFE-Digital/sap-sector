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
    private const string ComparatorSchoolUrn = "101230";
    private const string ComparatorSchoolName = "Roding Primary School";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(CurrentSchoolUrn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.PrimarySchool(CurrentSchoolUrn).Overview);
        await Page.GetByRole(AriaRole.Link, new() { Name = "View similar schools", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.PrimarySchool(CurrentSchoolUrn).ViewSimilarSchools);
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