using Microsoft.Playwright;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.EndToEnd.Secondary;

/// <summary>
/// Attendance and exclusion filters on the "View similar schools" page. Covers primary, secondary and
/// all-through schools as the filter panel is shared between them.
/// </summary>
[Collection("EndToEndTestsCollection")]
public class ViewSimilarSchoolsExclusionFiltersEndToEndTests(EndToEndTestsFixture fixture)
    : EndToEndTests(fixture)
{
    public static TheoryData<string, string, string, string> SchoolPages => new()
    {
        // Path, this school's suspension rate, percentage of pupils suspended, permanent exclusion rate
        { Routes.SecondarySchool("100051").ViewSimilarSchools, "4.05", "2.67%", "0.55" },
        { Routes.PrimarySchool("101206").ViewSimilarSchools, "1.56", "1.22%", "0.11" },
        { Routes.AllThroughSchool("100171").ViewSimilarSchools, "13.08", "7.38%", "0.12" },
        { $"{Routes.AllThroughSchool("100171").ViewSimilarSchools}?phase=secondary", "13.08", "7.38%", "0.12" },
    };

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task ExclusionFilters_ShownCollapsedInAttendanceAndExclusionSection(string path, string suspensionRate, string pupilsSuspended, string permanentExclusionRate)
    {
        await NavigateTo(path);

        var group = Page.Locator("fieldset.app-filter__fieldset", new() { Has = Page.Locator("legend", new() { HasTextString = "Attendance and exclusion" }) });
        await Expect(group.Locator(".app-filter-section__title")).ToHaveTextAsync([
            "Overall absence rate",
            "Persistent absence rate",
            "Suspension rate",
            "Percentage of pupils suspended",
            "Permanent exclusion rate"
        ]);

        foreach (var key in new[] { "oar", "par", "sr", "pps", "per" })
        {
            await Expect(Page.Locator($"button[aria-controls=\"filter-{key}\"]")).ToHaveAttributeAsync("aria-expanded", "false");
            await Expect(Page.Locator($"#filter-{key}")).ToBeHiddenAsync();
        }

        await ExpandFilter("sr");
        await ExpectFilterContent("sr", "Number of suspensions per 100 pupils", suspensionRate, hasPercentSuffix: false);

        await ExpandFilter("pps");
        await ExpectFilterContent("pps", "Percentage of pupils who were suspended at least once in a year", pupilsSuspended, hasPercentSuffix: true);

        await ExpandFilter("per");
        await ExpectFilterContent("per", "Number of permanent exclusions per 100 pupils", permanentExclusionRate, hasPercentSuffix: false);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task ExclusionFilters_ApplyRemoveAndClearFilters(string path, string _, string __, string ___)
    {
        await NavigateTo(path);
        var originalCount = await GetResultsCount();

        await ExpandFilter("sr");
        await Page.Locator("#sr_f").FillAsync("0");
        await Page.Locator("#sr_t").FillAsync("5");
        await ApplyFilters();

        var tag = Page.Locator(".app-filter__tag", new() { HasTextString = "Suspension rate from 0 to 5" });
        await Expect(tag).ToBeVisibleAsync();
        var filteredCount = await GetResultsCount();
        Assert.True(filteredCount < originalCount, $"Expected fewer than {originalCount} schools, found {filteredCount}");

        // Adding another filter narrows the results further (AND logic)
        await ExpandFilter("per");
        await Page.Locator("#per_t").FillAsync("0.1");
        await ApplyFilters();
        await Expect(Page.Locator(".app-filter__tag")).ToHaveCountAsync(2);
        Assert.True(await GetResultsCount() <= filteredCount);

        // Removing a tag removes only that filter
        await Page.Locator(".app-filter__tag", new() { HasTextString = "Permanent exclusion rate" }).ClickAsync();
        await Expect(Page.Locator(".app-filter__tag")).ToHaveCountAsync(1);
        Assert.Equal(filteredCount, await GetResultsCount());

        await Page.GetByRole(AriaRole.Link, new() { Name = "Clear filters" }).ClickAsync();
        await Expect(Page.Locator(".app-filter__tag")).ToHaveCountAsync(0);
        Assert.Equal(originalCount, await GetResultsCount());
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task ExclusionFilters_PercentageOfPupilsSuspended_ShowsPercentTag(string path, string _, string __, string ___)
    {
        await NavigateTo(path);

        await ExpandFilter("pps");
        await Page.Locator("#pps_f").FillAsync(".5");
        await ApplyFilters();

        await Expect(Page.Locator(".app-filter__tag", new() { HasTextString = "Percentage of pupils suspended over 0.5%" })).ToBeVisibleAsync();
        await Expect(Page.Locator("#pps_f")).ToHaveValueAsync("0.5");
    }

    [Theory]
    [InlineData("sr_f", "abc", "sr_f-error", "Enter a numeric value")]
    [InlineData("pps_t", "1..0", "pps_t-error", "Enter a numeric value")]
    public async Task ExclusionFilters_NonNumericValue_ShowsError(string inputId, string value, string errorId, string expectedError)
    {
        await NavigateTo(Routes.SecondarySchool("100051").ViewSimilarSchools);

        await ExpandFilter(inputId.Split('_')[0]);
        await Page.Locator($"#{inputId}").FillAsync(value);
        await ApplyFilters();

        await Expect(Page.Locator($"#{errorId}")).ToContainTextAsync(expectedError);
    }

    [Fact]
    public async Task ExclusionFilters_FromNotLowerThanTo_ShowsError()
    {
        await NavigateTo(Routes.PrimarySchool("101206").ViewSimilarSchools);

        await ExpandFilter("per");
        await Page.Locator("#per_f").FillAsync("5");
        await Page.Locator("#per_t").FillAsync("1");
        await ApplyFilters();

        await Expect(Page.Locator("#per-error")).ToContainTextAsync("The From value must be lower than the To value");
    }

    [Fact]
    public async Task ExclusionFilters_WhenNoSchoolsMatch_ShowsNoResultsMessage()
    {
        await NavigateTo(Routes.SecondarySchool("100051").ViewSimilarSchools);

        await ExpandFilter("sr");
        await Page.Locator("#sr_f").FillAsync("900");
        await ApplyFilters();

        await Expect(Page.Locator("#similar-schools-results")).ToHaveTextAsync("There are no schools that match your search.");
    }

    private async Task ExpandFilter(string key)
    {
        var toggle = Page.Locator($"button[aria-controls=\"filter-{key}\"]");
        if (await toggle.GetAttributeAsync("aria-expanded") != "true")
        {
            await toggle.ClickAsync();
        }

        await Expect(Page.Locator($"#filter-{key}")).ToBeVisibleAsync();
    }

    private async Task ExpectFilterContent(string key, string hint, string currentSchoolValue, bool hasPercentSuffix)
    {
        var content = Page.Locator($"#filter-{key}");
        await Expect(content.Locator(".govuk-hint")).ToHaveTextAsync(hint);
        await Expect(content.GetByLabel("From", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(content.GetByLabel("To", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(content.Locator(".govuk-inset-text")).ToHaveTextAsync($"This school: {currentSchoolValue}");

        var suffix = await content.Locator(".app-filter-range__input").First
            .EvaluateAsync<string>("e => getComputedStyle(e, '::after').display");
        Assert.Equal(hasPercentSuffix, suffix != "none");
    }

    private async Task ApplyFilters()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Apply filters" }).First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task<int> GetResultsCount()
    {
        var count = Page.Locator("#similar-schools-results-count strong").Last;
        if (await count.CountAsync() == 0)
        {
            return 0;
        }

        return int.Parse(await count.InnerTextAsync());
    }
}
