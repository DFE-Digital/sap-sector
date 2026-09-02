using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Secondary;

[Collection("EndToEndTestsCollection")]
public class ComparisonSchoolDetailsPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string CurrentSchoolUrn = "100052";
    private const string CurrentSchoolName = "Hampstead School";
    private const string ComparatorSchoolUrn = "141617";
    private const string ComparatorSchoolName = "The Hurlingham Academy";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(CurrentSchoolUrn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Overview);
        await Page.GetByText("View similar schools", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).ViewSimilarSchools);
        await Page.GetByText(ComparatorSchoolName, new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Comparison(ComparatorSchoolUrn).Similarity);
        await Page.GetByText("School details", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(Routes.SecondarySchool(CurrentSchoolUrn).Comparison(ComparatorSchoolUrn).SchoolDetails);
    }

    [Fact]
    public async Task Test()
    {
        await Expect(Page.GetByDefinitionTerm("ID")).ToContainTextAsync($"URN: {ComparatorSchoolUrn}");
        await Expect(Page.Locator(".govuk-caption-xl")).ToHaveTextAsync(CurrentSchoolName);
        await Expect(Page.Locator(".govuk-heading-xl")).ToHaveTextAsync(ComparatorSchoolName);
    }
}
