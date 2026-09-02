using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Primary;

[Collection("EndToEndTestsCollection")]
public class ComparisonSchoolDetailsPageEndToEndTests(EndToEndTestsFixture fixture)
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
        await Page.GetByRole(AriaRole.Link, new() { Name = "School details", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.PrimarySchool(CurrentSchoolUrn).Comparison(ComparatorSchoolUrn).SchoolDetails);
    }

    [Fact]
    public async Task Test()
    {
        await Expect(Page.GetByDefinitionTerm("ID")).ToContainTextAsync($"URN: {ComparatorSchoolUrn}");
        await Expect(Page.Locator(".govuk-caption-xl")).ToHaveTextAsync(CurrentSchoolName);
        await Expect(Page.Locator(".govuk-heading-xl")).ToHaveTextAsync(ComparatorSchoolName);
    }
}