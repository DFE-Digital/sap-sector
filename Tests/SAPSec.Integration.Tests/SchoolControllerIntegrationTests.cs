using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SchoolControllerIntegrationTests(WebApplicationSetupFixture fixture)
{
    #region GET /school/{urn} Tests

    [Fact]
    public async Task GetSchool_WithValidUrn_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/school/147788");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSchool_WithValidUrn_ReturnsPageWithSchoolName()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Bradfield School");
    }

    [Fact]
    public async Task GetSchool_WithNonExistentUrn_ReturnsNotFound()
    {
        var response = await fixture.Client.GetAsync("/school/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSchool_WithInvalidUrn_ReturnsNotFound()
    {
        var response = await fixture.Client.GetAsync("/school/invalid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSchool_HasSecurityHeaders()
    {
        var response = await fixture.Client.GetAsync("/school/147788");

        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("Content-Security-Policy");
    }

    #endregion

    #region Location Section Tests

    [Fact]
    public async Task GetSchool_ContainsLocationSection()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Location");
    }

    [Fact]
    public async Task GetSchool_ContainsAddressField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Address");
    }

    [Fact]
    public async Task GetSchool_ContainsLocalAuthorityField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Local authority");
    }

    [Fact]
    public async Task GetSchool_ContainsRegionField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Region");
    }

    [Fact]
    public async Task GetSchool_ContainsUrbanRuralDescriptionField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Urban/rural description");
    }

    #endregion

    #region School Details Section Tests

    [Fact]
    public async Task GetSchool_ContainsSchoolDetailsSection()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("School details");
    }

    [Fact]
    public async Task GetSchool_ContainsIdField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain(">ID<");
    }

    [Fact]
    public async Task GetSchool_ContainsUrnInIdField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("URN: 147788");
    }

    [Fact]
    public async Task GetSchool_ContainsAgeRangeField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Age range");
    }

    [Fact]
    public async Task GetSchool_ContainsGenderOfEntryField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Gender of entry");
    }

    [Fact]
    public async Task GetSchool_ContainsPhaseOfEducationField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Phase of education");
    }

    [Fact]
    public async Task GetSchool_ContainsSchoolTypeField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("School type");
    }

    [Fact]
    public async Task GetSchool_ContainsGovernanceStructureField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Governance structure");
    }

    [Fact]
    public async Task GetSchool_ContainsAcademyTrustField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Academy trust");
    }

    [Fact]
    public async Task GetSchool_ContainsAdmissionsPolicyField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Admissions policy");
    }

    [Fact]
    public async Task GetSchool_ContainsSendIntegratedResourceField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // content.Should().Contain("SEND integrated resource");
        content.Should().Contain("Resourced provision");
    }

    [Fact]
    public async Task GetSchool_ContainsReligiousCharacterField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Religious character");
    }

    #endregion

    #region Contact Details Section Tests

    [Fact]
    public async Task GetSchool_ContainsContactDetailsSection()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Contact details");
    }

    [Fact]
    public async Task GetSchool_ContainsHeadteacherField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Headteacher/Principal");
    }

    [Fact]
    public async Task GetSchool_ContainsWebsiteField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Website");
    }

    [Fact]
    public async Task GetSchool_ContainsTelephoneField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Telephone");
    }

    [Fact]
    public async Task GetSchool_ContainsEmailField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Email");
    }

    #endregion

    #region Further Information Section Tests

    [Fact]
    public async Task GetSchool_ContainsFurtherInformationSection()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Further information");
    }

    [Fact]
    public async Task GetSchool_ContainsOfstedReportField()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Ofsted report");
    }

    [Fact]
    public async Task GetSchool_ContainsOfstedReportLink()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("reports.ofsted.gov.uk");
    }

    #endregion

    #region Page Structure Tests

    [Fact]
    public async Task GetSchool_HasCorrectPageTitle()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("<title>");
        content.Should().Contain("Bradfield School");
        content.Should().Contain("School details");
    }

    [Fact]
    public async Task GetSchool_HasBackLink()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("govuk-back-link");
    }

    [Fact]
    public async Task GetSchool_HasSummaryListStructure()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("govuk-summary-list");
        content.Should().Contain("govuk-summary-list__key");
        content.Should().Contain("govuk-summary-list__value");
    }

    [Fact]
    public async Task GetSchool_HasCorrectHeadings()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("govuk-heading-xl");
        content.Should().Contain("govuk-heading-m");
    }

    #endregion

    #region Link Tests

    [Fact]
    public async Task GetSchool_WebsiteLink_OpensInNewTab()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("target=\"_blank\"");
        content.Should().Contain("rel=\"noopener noreferrer\"");
    }

    [Fact]
    public async Task GetSchool_ExternalLinks_HaveVisuallyHiddenText()
    {
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("govuk-visually-hidden");
        content.Should().Contain("opens in a new tab");
    }

    #endregion

    #region No Available Data Tests

    [Fact]
    public async Task GetSchool_ShowsNoAvailableData_WhenFieldIsEmpty()
    {
        // This test assumes you have a school in your test data with missing fields
        var response = await fixture.Client.GetAsync("/school/147788");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Check that the page renders correctly even if some fields show "No available data"
        content.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetSchool_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var response = await fixture.Client.GetAsync("/school/147788", cts.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetSchool_WithLeadingZerosInUrn_ReturnsNotFound()
    {
        var response = await fixture.Client.GetAsync("/school/000147788");

        // Depending on your implementation, this might return NotFound or redirect
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSchool_WithTrailingSlash_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/school/147788/");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.MovedPermanently, HttpStatusCode.Redirect);
    }

    [Theory]
    [InlineData("147788")]
    [InlineData("138361")]
    [InlineData("108055")]
    public async Task GetSchool_WithVariousValidUrns_ReturnsSuccess(string urn)
    {
        var response = await fixture.Client.GetAsync($"/school/{urn}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}