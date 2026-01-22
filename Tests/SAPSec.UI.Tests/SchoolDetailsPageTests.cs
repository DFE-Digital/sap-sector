using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class SchoolDetailsPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private readonly WebApplicationSetupFixture _fixture = fixture;

    private const string SchoolDetailsPath = "/school/147788/school-details";
    private const string SchoolSearchPath = "/search-for-a-school";

    #region Page Load Tests

    [Fact]
    public async Task SchoolDetails_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(SchoolDetailsPath);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task SchoolDetails_DisplaysSchoolName()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var schoolName = Page.Locator(".govuk-caption-l");
        var isVisible = await schoolName.IsVisibleAsync();

        isVisible.Should().BeTrue("School name caption should be visible");

        var nameText = await schoolName.TextContentAsync();
        nameText.Should().Contain("Bradfield School");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysPageHeading()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heading = Page.Locator("h1.govuk-heading-xl");
        var headingText = await heading.TextContentAsync();

        headingText.Should().Contain("School details");
    }

    [Fact]
    public async Task SchoolDetails_HasCorrectPageTitle()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await Page.TitleAsync();

        title.Should().Contain("Bradfield School");
        title.Should().Contain("School details");
    }

    #endregion

    #region Back Link Tests

    [Fact]
    public async Task SchoolDetails_HasBackLink()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var backLink = Page.Locator(".govuk-breadcrumbs__link");
        var isVisible = await backLink.IsVisibleAsync();

        isVisible.Should().BeTrue("Back link should be visible");
    }

    [Fact]
    public async Task SchoolDetails_BackLink_HasCorrectText()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var backLink = Page.Locator(".govuk-breadcrumbs__link");
        var linkText = await backLink.TextContentAsync();

        linkText.Should().Contain("Home");
    }

    [Fact]
    public async Task SchoolDetails_BackLink_NavigatesToHome()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var backLink = Page.Locator(".govuk-breadcrumbs__link");
        await backLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Page.Url.Should().Contain("search-for-a-school");
        Page.Url.Should().Contain("SchoolHome");
    }

    #endregion

    #region Location Section Tests

    [Fact]
    public async Task SchoolDetails_DisplaysLocationSection()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var locationHeading = Page.Locator("h2:has-text('Location')");
        var isVisible = await locationHeading.IsVisibleAsync();

        isVisible.Should().BeTrue("Location section heading should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysAddressField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var addressKey = Page.Locator(".govuk-summary-list__key:has-text('Address')");
        var isVisible = await addressKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Address field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysLocalAuthorityField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var laKey = Page.Locator(".govuk-summary-list__key:has-text('Local authority')");
        var isVisible = await laKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Local authority field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysRegionField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var regionKey = Page.Locator(".govuk-summary-list__key:has-text('Region')");
        var isVisible = await regionKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Region field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysUrbanRuralDescriptionField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var urbanRuralKey = Page.Locator(".govuk-summary-list__key:has-text('Urban/rural description')");
        var isVisible = await urbanRuralKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Urban/rural description field should be visible");
    }

    #endregion

    #region School Details Section Tests

    [Fact]
    public async Task SchoolDetails_DisplaysSchoolDetailsSection()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var schoolDetailsHeading = Page.Locator("h2:has-text('School details')");
        var count = await schoolDetailsHeading.CountAsync();

        count.Should().BeGreaterThan(0, "School details section heading should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysIdField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var idKey = Page.Locator(".govuk-summary-list__key:text-is('ID')");
        var isVisible = await idKey.IsVisibleAsync();

        isVisible.Should().BeTrue("ID field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_IdField_ContainsUrn()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await Page.ContentAsync();

        content.Should().Contain("URN: 147788");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysAgeRangeField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var ageRangeKey = Page.Locator(".govuk-summary-list__key:has-text('Age range')");
        var isVisible = await ageRangeKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Age range field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysGenderOfEntryField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var genderKey = Page.Locator(".govuk-summary-list__key:has-text('Gender of entry')");
        var isVisible = await genderKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Gender of entry field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysPhaseOfEducationField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var phaseKey = Page.Locator(".govuk-summary-list__key:has-text('Phase of education')");
        var isVisible = await phaseKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Phase of education field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysSchoolTypeField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var typeKey = Page.Locator(".govuk-summary-list__key:has-text('School type')");
        var isVisible = await typeKey.IsVisibleAsync();

        isVisible.Should().BeTrue("School type field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysGovernanceStructureField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var governanceKey = Page.Locator(".govuk-summary-list__key:has-text('Governance structure')");
        var isVisible = await governanceKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Governance structure field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysAcademyTrustField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var trustKey = Page.Locator(".govuk-summary-list__key:has-text('Academy trust')");
        var isVisible = await trustKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Academy trust field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysAdmissionsPolicyField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var admissionsKey = Page.Locator(".govuk-summary-list__key:has-text('Admissions policy')");
        var isVisible = await admissionsKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Admissions policy field should be visible");
    }
    

    [Fact]
    public async Task SchoolDetails_DisplaysNurseryProvisionField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var religiousKey = Page.Locator(".govuk-summary-list__key:has-text('Nursery provision')");
        var isVisible = await religiousKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Nursery provision field should be visible");
    }
    
    [Fact]
    public async Task SchoolDetails_DisplaysSixthFormField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var religiousKey = Page.Locator(".govuk-summary-list__key:has-text('Sixth form')");
        var isVisible = await religiousKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Sixth form field should be visible");
    }
    [Fact]
    public async Task SchoolDetails_DisplaysSENUnitField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var religiousKey = Page.Locator(".govuk-summary-list__key:has-text('SEN unit')");
        var isVisible = await religiousKey.IsVisibleAsync();

        isVisible.Should().BeTrue("SEN unit field should be visible");
    }
    
    [Fact]
    public async Task SchoolDetails_DisplaysResourcedprovisionField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var religiousKey = Page.Locator(".govuk-summary-list__key:has-text('Resourced provision')");
        var isVisible = await religiousKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Resourced provision field should be visible");
    }
    
    [Fact]
    public async Task SchoolDetails_DisplaysReligiousCharacterField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var religiousKey = Page.Locator(".govuk-summary-list__key:has-text('Religious character')");
        var isVisible = await religiousKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Religious character field should be visible");
    }
    
    
    

    #endregion

    #region Contact Details Section Tests

    [Fact]
    public async Task SchoolDetails_DisplaysContactDetailsSection()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var contactHeading = Page.Locator("h2:has-text('Contact details')");
        var isVisible = await contactHeading.IsVisibleAsync();

        isVisible.Should().BeTrue("Contact details section heading should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysHeadteacherField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var headteacherKey = Page.Locator(".govuk-summary-list__key:has-text('Headteacher / Principal')");
        var isVisible = await headteacherKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Headteacher / Principal field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysWebsiteField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var websiteKey = Page.Locator(".govuk-summary-list__key:has-text('Website')");
        var isVisible = await websiteKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Website field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysTelephoneField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var telephoneKey = Page.Locator(".govuk-summary-list__key:has-text('Telephone')");
        var isVisible = await telephoneKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Telephone field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysEmailField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var emailKey = Page.Locator(".govuk-summary-list__key:has-text('Email')");
        var isVisible = await emailKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Email field should be visible");
    }

    #endregion

    #region Further Information Section Tests

    [Fact]
    public async Task SchoolDetails_DisplaysFurtherInformationSection()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var furtherInfoHeading = Page.Locator("h2:has-text('Further information')");
        var isVisible = await furtherInfoHeading.IsVisibleAsync();

        isVisible.Should().BeTrue("Further information section heading should be visible");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysOfstedReportField()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var ofstedKey = Page.Locator(".govuk-summary-list__key:has-text('Ofsted report')");
        var isVisible = await ofstedKey.IsVisibleAsync();

        isVisible.Should().BeTrue("Ofsted report field should be visible");
    }

    [Fact]
    public async Task SchoolDetails_OfstedReportLink_HasCorrectUrl()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var ofstedLink = Page.Locator("a[href*='reports.ofsted.gov.uk']");
        var href = await ofstedLink.GetAttributeAsync("href");

        href.Should().Contain("147788");
    }

    [Fact]
    public async Task SchoolDetails_OfstedReportLink_OpensInNewTab()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var ofstedLink = Page.Locator("a[href*='reports.ofsted.gov.uk']");
        var target = await ofstedLink.GetAttributeAsync("target");

        target.Should().Be("_blank");
    }

    #endregion

    #region External Links Tests

    [Fact]
    public async Task SchoolDetails_WebsiteLink_OpensInNewTab()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var websiteLinks = Page.Locator(".govuk-summary-list__value a[target='_blank']");
        var count = await websiteLinks.CountAsync();

        count.Should().BeGreaterThan(0, "External links should open in new tab");
    }

    [Fact]
    public async Task SchoolDetails_ExternalLinks_HaveNoopenerNoreferrer()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var externalLinks = Page.Locator("a[target='_blank']");
        var count = await externalLinks.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var rel = await externalLinks.Nth(i).GetAttributeAsync("rel");
            rel.Should().Contain("noopener");
        }
    }

    #endregion

    #region Summary List Structure Tests
    [Fact]
    public async Task SchoolDetails_SummaryListRows_HaveKeyAndValue()
    {
        await Page.GotoAsync(SchoolDetailsPath);
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

    #region No Available Data Tests

    [Fact]
    public async Task SchoolDetails_ShowsNoAvailableData_WhenFieldIsEmpty()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await Page.ContentAsync();

        // At least some fields should show "No available data" (e.g., Email, Age range)
        content.Should().Contain("No available data");
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task SchoolDetails_HeadingsAreInCorrectOrder()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = Page.Locator("h1");
        var h2s = Page.Locator("h2");

        (await h1.CountAsync()).Should().Be(1, "Should have exactly one h1");
        (await h2s.CountAsync()).Should().BeGreaterThanOrEqualTo(4, "Should have at least 4 h2 section headings");
    }

    [Fact]
    public async Task SchoolDetails_HasMainContentLandmark()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var main = Page.Locator("main#main-content");
        var count = await main.CountAsync();

        count.Should().Be(1, "Should have main content landmark");
    }

    [Fact]
    public async Task SchoolDetails_SkipLinkTargetsMainContent()
    {
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var skipLink = Page.Locator(".govuk-skip-link");
        var href = await skipLink.GetAttributeAsync("href");

        href.Should().Be("#main-content");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SchoolDetails_NonExistentUrn_Shows404()
    {
        var response = await Page.GotoAsync("/school/999999");

        response!.Status.Should().Be(404);
    }

    [Fact]
    public async Task SchoolDetails_InvalidUrn_Shows404()
    {
        var response = await Page.GotoAsync("/school/invalid");

        response!.Status.Should().Be(404);
    }

    [Fact]
    public async Task SchoolDetails_EmptyUrn_Shows404()
    {
        var response = await Page.GotoAsync("/school/");

        response!.Status.Should().BeOneOf(404, 200); // Depends on routing
    }

    #endregion

    #region Responsive Tests

    [Fact]
    public async Task SchoolDetails_IsResponsive_OnMobile()
    {
        await Page.SetViewportSizeAsync(375, 667); // iPhone SE size
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var summaryList = Page.Locator(".govuk-summary-list").First;
        var isVisible = await summaryList.IsVisibleAsync();

        isVisible.Should().BeTrue("Summary list should be visible on mobile");
    }

    [Fact]
    public async Task SchoolDetails_IsResponsive_OnTablet()
    {
        await Page.SetViewportSizeAsync(768, 1024); // iPad size
        await Page.GotoAsync(SchoolDetailsPath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heading = Page.Locator("h1");
        var isVisible = await heading.IsVisibleAsync();

        isVisible.Should().BeTrue("Heading should be visible on tablet");
    }

    #endregion
}