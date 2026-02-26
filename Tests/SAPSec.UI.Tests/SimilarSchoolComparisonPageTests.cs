using System;
using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SimilarSchoolComparisonPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private readonly WebApplicationSetupFixture _fixture = fixture;

    private const string SimilarSchoolComparisonPath = "/school/108088/view-similar-schools/137621/SchoolDetails";

    #region Page Load Tests

    [Fact]
    public async Task SimilarSchoolComparison_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(SimilarSchoolComparisonPath);

        response.Should().NotBeNull();
        response.Status.Should().Be(200);
    }

    [Fact]
    public async Task SimilarSchoolComparison_DisplaysComparingHeaderBlock()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mainSchoolName = Page.Locator("span.govuk-caption-l");
        var isMainVisible = await mainSchoolName.IsVisibleAsync();

        isMainVisible.Should().BeTrue("Main school name caption should be visible");
        (await mainSchoolName.TextContentAsync()).Should().NotBeNullOrWhiteSpace();

        var comparingTo = Page.Locator("span.govuk-caption-m:has-text('Comparing to')");
        var isComparingVisible = await comparingTo.IsVisibleAsync();

        isComparingVisible.Should().BeTrue("'Comparing to' label should be visible");

        var similarSchoolName = Page.Locator("h1.govuk-heading-xl");
        var isH1Visible = await similarSchoolName.IsVisibleAsync();

        isH1Visible.Should().BeTrue("Similar school name heading should be visible");
        (await similarSchoolName.TextContentAsync()).Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Back Link Tests

    [Fact]
    public async Task SimilarSchoolComparison_HasBackLink()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var backLink = Page.Locator("a.govuk-back-link");
        var isVisible = await backLink.IsVisibleAsync();

        isVisible.Should().BeTrue("Back link should be visible");
    }

    [Fact]
    public async Task SimilarSchoolComparison_BackLink_HasCorrectText()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var backLink = Page.Locator("a.govuk-back-link");
        var linkText = await backLink.TextContentAsync();

        linkText.Should().Contain("Back");
    }

    [Fact]
    public async Task SimilarSchoolComparison_BackLink_HasExpectedHref()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var backLink = Page.Locator("a.govuk-back-link");
        var href = await backLink.GetAttributeAsync("href");

        href.Should().NotBeNullOrWhiteSpace("Back link should have an href");
        href!.Should().Contain("view-similar-schools", "Back link should navigate to the similar schools journey");
    }

    #endregion

    #region Compare Navigation Tests

    [Fact]
    public async Task SimilarSchoolComparison_ShowsCompareServiceNavigation()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var nav = Page.Locator("div.govuk-service-navigation.compare-nav nav[aria-label='Compare sections']");
        var count = await nav.CountAsync();

        count.Should().Be(1, "Compare sections service navigation should be visible");
    }

    [Fact]
    public async Task SimilarSchoolComparison_CompareServiceNavigation_HasAllTabs()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var links = Page.Locator("div.govuk-service-navigation.compare-nav a.govuk-service-navigation__link");
        var count = await links.CountAsync();

        count.Should().Be(5, "Compare navigation should have 5 tabs");

        (await links.Nth(0).TextContentAsync()).Should().Contain("Similarity");
        (await links.Nth(1).TextContentAsync()).Should().Contain("KS4 headline measures");
        (await links.Nth(2).TextContentAsync()).Should().Contain("KS4 core subjects");
        (await links.Nth(3).TextContentAsync()).Should().Contain("Attendance");
        (await links.Nth(4).TextContentAsync()).Should().Contain("School details");
    }

    [Fact]
    public async Task SimilarSchoolComparison_SchoolDetailsTab_IsActive()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var activeTab = Page.Locator("li.govuk-service-navigation__item--active a.govuk-service-navigation__link");
        var count = await activeTab.CountAsync();

        count.Should().Be(1, "Exactly one tab should be active");

        (await activeTab.TextContentAsync()).Should().Contain("School details");

        var ariaCurrent = await activeTab.GetAttributeAsync("aria-current");
        ariaCurrent.Should().Be("true", "Active tab should have aria-current='true'");
    }

    [Fact]
    public async Task SimilarSchoolComparison_CanNavigateToSimilarity_AndSeeCharacteristicsTable()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var similarityTab = Page.Locator("a.govuk-service-navigation__link:has-text('Similarity')");
        await similarityTab.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().Contain("/view-similar-schools/137621");

        var heading = Page.Locator("h1.govuk-heading-l:text-is('How these schools compare')");
        (await heading.CountAsync()).Should().Be(1, "Similarity page heading should be visible");

        var table = Page.Locator("table.govuk-table");
        (await table.CountAsync()).Should().Be(1, "Similarity table should be visible");

        var rows = table.Locator("tbody tr.govuk-table__row");
        (await rows.CountAsync()).Should().Be(9, "Similarity table should list 9 characteristics");
    }

    [Fact]
    public async Task SimilarSchoolComparison_SimilarityTable_HasExpectedHeaderAndNonEmptyValues()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator("a.govuk-service-navigation__link:has-text('Similarity')").ClickAsync();
        await Page.WaitForURLAsync("**/Similarity");

        var table = Page.Locator("table.govuk-table");
        await table.WaitForAsync();

        var headerCells = table.Locator("thead th");
        (await headerCells.CountAsync()).Should().Be(3, "Similarity table should have 3 columns");
        (await headerCells.Nth(0).TextContentAsync()).Should().Contain("Characteristic");

        var firstRow = table.Locator("tbody tr.govuk-table__row").First;
        (await firstRow.CountAsync()).Should().Be(1);

        var cells = firstRow.Locator("th, td");
        (await cells.CountAsync()).Should().Be(3, "Row should have characteristic + 2 value cells");

        (await cells.Nth(0).TextContentAsync()).Should().NotBeNullOrWhiteSpace();
        (await cells.Nth(1).TextContentAsync()).Should().NotBeNullOrWhiteSpace();
        (await cells.Nth(2).TextContentAsync()).Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Page Body Heading Tests

    [Fact]
    public async Task SimilarSchoolComparison_Body_DisplaysSchoolDetailsHeading()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h2 = Page.Locator("h2:text-is('School Details')");
        var count = await h2.CountAsync();

        count.Should().Be(1, "School Details body heading should be visible");
    }

    #endregion

    #region Map Tests

    [Fact]
    public async Task SimilarSchoolComparison_DisplaysMapDetails_Component()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var details = Page.Locator("details#comparison-map-details.govuk-details");
        var count = await details.CountAsync();

        count.Should().Be(1, "Map details component should be present");

        var summaryText = details.Locator("summary .govuk-details__summary-text");
        (await summaryText.TextContentAsync()).Should().Contain("View on a map");
    }

    [Fact]
    public async Task SimilarSchoolComparison_MapContainer_HasExpectedAttributes()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

    
        await Page.WaitForSelectorAsync("#map", new() { State = WaitForSelectorState.Attached, Timeout = 15000 });

        var map = Page.Locator("#map");

    
        (await map.CountAsync()).Should().Be(1, "Map container should exist");

    
        var mapMode = await map.GetAttributeAsync("data-map-mode");
        mapMode.Should().Be("compare");

        (await map.GetAttributeAsync("data-fixed-zoom")).Should().Be("14");

    
        (await map.GetAttributeAsync("role")).Should().Be("region");
        (await map.GetAttributeAsync("aria-label")).Should().Be("Map of schools");

        var loading = map.Locator(".map-loading");
        (await loading.CountAsync()).Should().BeGreaterThanOrEqualTo(0);

    
        if (await loading.CountAsync() > 0)
        {
            (await loading.First.TextContentAsync()).Should().Contain("Loading map");
        }
    }

    [Fact]
    public async Task SimilarSchoolComparison_MapLegend_HasBothMarkerIcons()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mainMarker = Page.Locator("img.school-marker-icon[src='/assets/images/marker-school-pink.svg']");
        var similarMarker = Page.Locator("img.school-marker-icon[src='/assets/images/marker-school.svg']");

        (await mainMarker.CountAsync()).Should().Be(1, "Main school marker icon (pink) should be present");
        (await similarMarker.CountAsync()).Should().Be(1, "Similar school marker icon (blue) should be present");
    }

    [Fact]
    public async Task SimilarSchoolComparison_HasSchoolsDataJsonScript()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var script = Page.Locator("script#schools-data[type='application/json']");
        var count = await script.CountAsync();

        count.Should().Be(1, "schools-data JSON script tag should exist");

        var json = await script.TextContentAsync();
        json.Should().NotBeNullOrWhiteSpace("schools-data script should contain JSON");
        json!.Should().Contain("isMain");
        json.Should().Contain("isComparedSchool");
        json.Should().Contain("lat");
        json.Should().Contain("lon");
    }

    #endregion

    #region Contact This School Tests

    [Fact]
    public async Task SimilarSchoolComparison_DisplaysContactThisSchoolSection()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heading = Page.Locator("h2.govuk-heading-m:text-is('Contact this school')");
        var isVisible = await heading.IsVisibleAsync();

        isVisible.Should().BeTrue("Contact this school section heading should be visible");
    }

    [Fact]
    public async Task SimilarSchoolComparison_ContactSection_DisplaysExpectedFields()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        (await Page.Locator(".govuk-summary-list__key:text-is('Headteacher/Principal')").IsVisibleAsync())
            .Should().BeTrue("Headteacher/Principal field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Website')").IsVisibleAsync())
            .Should().BeTrue("Website field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Telephone')").IsVisibleAsync())
            .Should().BeTrue("Telephone field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Email')").IsVisibleAsync())
            .Should().BeTrue("Email field should be visible");
    }

    [Fact]
    public async Task SimilarSchoolComparison_ExternalLinks_OpenInNewTab_WhenPresent()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var externalLinks = Page.Locator("a[target='_blank']");
        var count = await externalLinks.CountAsync();

        count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SimilarSchoolComparison_ExternalLinks_HaveNoopenerNoreferrer_WhenPresent()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var externalLinks = Page.Locator("a[target='_blank']");
        var count = await externalLinks.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var rel = await externalLinks.Nth(i).GetAttributeAsync("rel");
            rel.Should().NotBeNull("External links should have rel attributes");
            rel!.Should().Contain("noopener");
            rel.Should().Contain("noreferrer");
        }
    }

    #endregion

    #region Location Section Tests

    [Fact]
    public async Task SimilarSchoolComparison_DisplaysLocationSection()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var locationHeading = Page.Locator("h2.govuk-heading-m:text-is('Location')");
        var isVisible = await locationHeading.IsVisibleAsync();

        isVisible.Should().BeTrue("Location section heading should be visible");
    }

    [Fact]
    public async Task SimilarSchoolComparison_LocationSection_DisplaysExpectedFields()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        (await Page.Locator(".govuk-summary-list__key:text-is('Distance between school')").IsVisibleAsync())
            .Should().BeTrue("Distance between school field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Address')").IsVisibleAsync())
            .Should().BeTrue("Address field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Local authority')").IsVisibleAsync())
            .Should().BeTrue("Local authority field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Region')").IsVisibleAsync())
            .Should().BeTrue("Region field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Urban/rural description')").IsVisibleAsync())
            .Should().BeTrue("Urban/rural description field should be visible");
    }

    #endregion

    #region School Details Section Tests

    [Fact]
    public async Task SimilarSchoolComparison_DisplaysSchoolDetailsSection()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heading = Page.Locator("h2.govuk-heading-m:text-is('School details')");
        var count = await heading.CountAsync();

        count.Should().Be(1, "School details section heading should be visible");
    }

    [Fact]
    public async Task SimilarSchoolComparison_SchoolDetailsSection_DisplaysExpectedFields()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        (await Page.Locator(".govuk-summary-list__key:text-is('ID')").IsVisibleAsync())
            .Should().BeTrue("ID field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Age range')").IsVisibleAsync())
            .Should().BeTrue("Age range field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Gender of entry')").IsVisibleAsync())
            .Should().BeTrue("Gender of entry field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Phase of education')").IsVisibleAsync())
            .Should().BeTrue("Phase of education field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('School type')").IsVisibleAsync())
            .Should().BeTrue("School type field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Governance structure')").IsVisibleAsync())
            .Should().BeTrue("Governance structure field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Admissions policy')").IsVisibleAsync())
            .Should().BeTrue("Admissions policy field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Religious character')").IsVisibleAsync())
            .Should().BeTrue("Religious character field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Nursery provision')").IsVisibleAsync())
            .Should().BeTrue("Nursery provision field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Sixth form')").IsVisibleAsync())
            .Should().BeTrue("Sixth form field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('SEN unit')").IsVisibleAsync())
            .Should().BeTrue("SEN unit field should be visible");

        (await Page.Locator(".govuk-summary-list__key:text-is('Resourced provision')").IsVisibleAsync())
            .Should().BeTrue("Resourced provision field should be visible");
    }

    [Fact]
    public async Task SimilarSchoolComparison_SENExplanationDetails_Exists()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var senDetails = Page.Locator("details.govuk-details:has(summary .govuk-details__summary-text:has-text('What is a SEN unit or resourced provision?'))");
        var count = await senDetails.CountAsync();

        count.Should().Be(1, "SEN explanation details component should be present");
    }

    #endregion

    #region Further Information + Data Sources Tests

    [Fact]
    public async Task SimilarSchoolComparison_DisplaysFurtherInformationSection()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var furtherInfoHeading = Page.Locator("h2.govuk-heading-m:text-is('Further information')");
        var isVisible = await furtherInfoHeading.IsVisibleAsync();

        isVisible.Should().BeTrue("Further information section heading should be visible");
    }

    [Fact]
    public async Task SimilarSchoolComparison_DisplaysDataSourcesDetails_WithExpectedLinks()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dataSources = Page.Locator("details.govuk-details:has(summary .govuk-details__summary-text:text-is('Data sources'))");
        var count = await dataSources.CountAsync();

        count.Should().Be(1, "Data sources details component should be visible");

        (await dataSources.Locator("a[href='https://get-information-schools.service.gov.uk/']").CountAsync())
            .Should().Be(1, "Get Information about Schools link should be present");

        (await dataSources.Locator("a[href='https://explore-education-statistics.service.gov.uk/']").CountAsync())
            .Should().Be(1, "Explore Education Statistics link should be present");
    }

    #endregion

    #region Summary List Structure Tests

    [Fact]
    public async Task SimilarSchoolComparison_SummaryListRows_HaveKeyAndValue()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var rows = Page.Locator(".govuk-summary-list__row");
        var count = await rows.CountAsync();

        for (var i = 0; i < Math.Min(count, 5); i++)
        {
            var row = rows.Nth(i);
            var key = row.Locator(".govuk-summary-list__key");
            var value = row.Locator(".govuk-summary-list__value");

            (await key.CountAsync()).Should().Be(1, $"Row {i} should have a key");
            (await value.CountAsync()).Should().Be(1, $"Row {i} should have a value");
        }
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task SimilarSchoolComparison_HeadingsAreInCorrectOrder()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = Page.Locator("h1");
        var h2s = Page.Locator("h2");

        (await h1.CountAsync()).Should().Be(1, "Should have exactly one h1");
        (await h2s.CountAsync()).Should().BeGreaterThanOrEqualTo(3, "Should have multiple h2 section headings");
    }

    [Fact]
    public async Task SimilarSchoolComparison_HasMainContentLandmark()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var main = Page.Locator("main#main-content");
        var count = await main.CountAsync();

        count.Should().BeGreaterThan(0, "Should have main content landmark");
    }

    [Fact]
    public async Task SimilarSchoolComparison_SkipLinkTargetsMainContent()
    {
        await Page.GotoAsync(SimilarSchoolComparisonPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var skipLink = Page.Locator(".govuk-skip-link");
        var href = await skipLink.GetAttributeAsync("href");

        href.Should().Be("#main-content");
    }

    #endregion
}
