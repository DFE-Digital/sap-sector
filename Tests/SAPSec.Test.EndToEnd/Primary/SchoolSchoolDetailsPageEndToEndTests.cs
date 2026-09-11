using Microsoft.Playwright;
using SAPSec.Test.Common.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Primary;

[Collection("EndToEndTestsCollection")]
public class SchoolSchoolDetailsPageEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    private const string CurrentSchoolUrn = "101206";
    private const string CurrentSchoolName = "Grafton Primary School";

    private static readonly Routes.Primary PrimarySchoolRoute = Routes.PrimarySchool(CurrentSchoolUrn);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await NavigateTo(Routes.FindASchool());
        await Page.GetByLabel("Get school improvement insights", new() { Exact = true }).FillAsync(CurrentSchoolUrn);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.Overview);
        await Page.GetByText("School details", new() { Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(PrimarySchoolRoute.SchoolDetails);
    }

    [Fact]
    public async Task Test()
    {
        await Expect(Page.GetByDefinitionTerm("ID")).ToContainTextAsync($"URN: {CurrentSchoolUrn}");
        await Expect(Page.Locator(".govuk-caption-xl")).ToHaveTextAsync(CurrentSchoolName);
    }
}