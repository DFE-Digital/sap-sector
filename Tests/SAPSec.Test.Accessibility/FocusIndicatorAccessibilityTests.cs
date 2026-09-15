using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

[Collection("AccessibilityTestsCollection")]
public class FocusIndicatorAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    private const string ExpectedFocusBackground = "rgb(255, 221, 0)";
    private const string ExpectedFocusForeground = "rgb(11, 12, 12)";
    private const string SideNavigationSelector = ".app-side-navigation__link";
    private const string SimilarSchoolsFilterSelector = ".app-filter-section__toggle";
    private const string MapClusterSelector = "#map .marker-cluster[data-map-focusable='true']";

    public static TheoryData<string> SideNavigationPages => new()
    {
        { Routes.SecondarySchool("100182").Overview },
        { Routes.SecondarySchool("100182").KS4HeadlineMeasures },
        { Routes.SecondarySchool("100182").KS4CoreSubjects },
        { Routes.SecondarySchool("100182").Attendance },
        { Routes.SecondarySchool("100182").ViewSimilarSchools },
        { Routes.SecondarySchool("100182").SchoolDetails },
        { Routes.SecondarySchool("100182").WhatIsASimilarSchool },
    };

    [Theory]
    [MemberData(nameof(SideNavigationPages))]
    public async Task SideNavigationLinks_UseServiceFocusStyle(string path)
    {
        await NavigateTo(path);

        await AssertUsesServiceFocusStyle(Page.Locator(SideNavigationSelector).First);
    }

    [Fact]
    public async Task SimilarSchoolsFilterSectionToggle_UsesServiceFocusStyle()
    {
        await NavigateTo(Routes.SecondarySchool("100182").ViewSimilarSchools);

        await AssertUsesServiceFocusStyle(Page.Locator(SimilarSchoolsFilterSelector).First);
    }

    [Fact]
    public async Task SchoolSearch_AutocompleteInputText_RemainsVisible_InForcedColorsMode()
    {
        await Page.EmulateMediaAsync(new() { ForcedColors = ForcedColors.Active });
        await NavigateTo(Routes.FindASchool());

        var input = Page.Locator("input[name='__Query'], input[name='Query']").First;
        await input.FocusAsync();
        await input.FillAsync("Test School");

        var styles = await input.EvaluateAsync<string[]>(@"
            el => {
                const styles = window.getComputedStyle(el);
                return [
                    styles.color,
                    styles.backgroundColor,
                    styles.webkitTextFillColor || styles.color
                ];
            }
        ");

        styles[0].Should().NotBe(styles[1]);
        styles[2].Should().NotBe(styles[1]);
    }

    [Fact]
    public async Task SchoolSearch_MapClusters_HaveDescriptiveAccessibleNames()
    {
        await NavigateTo(Routes.FindASchool("School"));

        await Page.Locator("#toggleViewLink").ClickAsync();
        await Page.WaitForTimeoutAsync(1500);

        var clusterLabels = await Page.Locator(MapClusterSelector).EvaluateAllAsync<string[]>(@"
            elements => elements
                .map(element => element.getAttribute('aria-label') || '')
                .filter(label => label.length > 0)
        ");

        clusterLabels.Should().NotBeEmpty();
        clusterLabels.Should().OnlyContain(label => label.Contains("including"));
    }

    private static async Task<string[]> GetFocusStyles(ILocator element)
    {
        await element.FocusAsync();

        return await element.EvaluateAsync<string[]>(@"
            el => {
                const styles = window.getComputedStyle(el);
                return [styles.backgroundColor, styles.boxShadow, styles.color];
            }
        ");
    }

    private static async Task AssertUsesServiceFocusStyle(ILocator element)
    {
        var styles = await GetFocusStyles(element);

        styles[0].Should().Be(ExpectedFocusBackground);
        styles[1].Should().Contain(ExpectedFocusForeground);
        styles[2].Should().Be(ExpectedFocusForeground);
    }
}
