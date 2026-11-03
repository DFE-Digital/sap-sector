using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class MapToggleAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    private static readonly string SimilarSchoolsPath = Routes.SecondarySchool("108088").ViewSimilarSchools;

    [Theory]
    [MemberData(nameof(MapTogglePages))]
    public async Task ToggleBackToList_FocusesResultsSoNextTabReachesFirstSchool(string path, string assertionReason)
    {
        await NavigateTo(path);
        await Page.EvaluateAsync("sessionStorage.clear()");
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggleLink = Page.Locator("#toggleViewLink");

        await toggleLink.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(500);

        await toggleLink.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Page.WaitForFunctionAsync("() => document.activeElement?.classList.contains('app-school-results')");

        await Page.Keyboard.PressAsync("Tab");

        var firstSchoolLink = Page.Locator("#listView .app-school-results a").First;
        var firstSchoolLinkIsFocused = await firstSchoolLink.EvaluateAsync<bool>("el => el === document.activeElement");

        firstSchoolLinkIsFocused.Should().BeTrue(assertionReason);
    }

    public static TheoryData<string, string> MapTogglePages()
    {
        return new TheoryData<string, string>
        {
            {
                Routes.FindASchool("School"),
                "tabbing after returning to list view should continue into the restored results"
            },
            {
                SimilarSchoolsPath,
                "tabbing after returning to list view should continue into the restored similar schools results"
            }
        };
    }
}
