using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Secondary;

[Collection("EndToEndTestsCollection")]
public class SchoolSchoolDetailsPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string CurrentSchoolUrn = "100051";
    private const string CurrentSchoolName = "Regent High School";

    private static readonly Routes.Secondary SecondarySchoolRoute = Routes.SecondarySchool(CurrentSchoolUrn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(CurrentSchoolUrn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.Overview);
        await Page.GetByText("School details", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(SecondarySchoolRoute.SchoolDetails);
    }

    [Fact]
    public async Task Test()
    {
        await Expect(Page.GetByDefinitionTerm("ID")).ToContainTextAsync($"URN: {CurrentSchoolUrn}");
        await Expect(Page.Locator(".govuk-caption-xl")).ToHaveTextAsync(CurrentSchoolName);
    }
}