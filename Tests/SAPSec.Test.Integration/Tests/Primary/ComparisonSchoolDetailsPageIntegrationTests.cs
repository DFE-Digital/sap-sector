using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class ComparisonSchoolDetailsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnablePrimarySchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task NonExistentCurrentSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("999999").Comparison("100002").SchoolDetails, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NonExistentComparatorSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("999999").SchoolDetails, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenComparatorSchoolIsNotInSimilarSchoolsGroupForCurrentSchool_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HeadingAndTitle_ReflectCurrentAndComparatorSchools()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        page.Title.Should().Be("Comparator School - Get school improvement insights - GOV.UK");

        var heading = page.ElementShouldExist("h1.govuk-heading-xl");
        heading.TrimmedTextContent().Should().Be("Comparator School");

        var caption = page.ElementShouldExist(".govuk-caption-xl");
        caption.TrimmedTextContent().Should().Be("Current School");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysContactThisSchoolSection()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        page.ElementWithTextContentShouldExist("h2.govuk-heading-m", "Contact this school");
    }

    [Fact]
    public async Task ContactDetailsSection_ContainsSchoolContactDetails()
    {
        Fixture.EstablishmentRepository
            .SetupEstablishments(
                Build.Establishment("100001", "Current School", x => x
                    .Open().Primary()),
                Build.Establishment("100002", "Comparator School", x => x
                    .Open().Primary()
                    .WithHeadTeacher("Miss", "Jane", "Smith")
                    .WithTelephone("01234 567890")
                    .WithWebsite("https://similar-school.example.com")))
            .SetupEstablishmentEmails(
                Build.EstablishmentEmail("100002", "test@education.gov.uk"));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        page.TermShouldExistWithDefinition("Headteacher/Principal", "Miss Jane Smith");
        page.TermShouldExistWithDefinition("Website", el => el.ChildElementShouldExist("a")
            .ShouldLinkTo("https://similar-school.example.com (opens in new tab)", "https://similar-school.example.com"));
        page.TermShouldExistWithDefinition("Telephone", "01234 567890");
        page.TermShouldExistWithDefinition("Email", el => el.ChildElementShouldExist("a")
            .ShouldLinkTo("test@education.gov.uk", "mailto:test@education.gov.uk"));
    }

    [Fact]
    public async Task LocationSection_ContainsSchoolLocationDetails()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x
                .Open().Primary()
                .WithAddress("1 The Street", "The Locality", "Address 3", "The Town", "TT1 1TT")
                .InLA("321", "Sheffield")
                .InAdministrativeDistrict("1", "The North")
                .WithUrbanRural("2", "Urban")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        page.TermShouldExistWithDefinition("Address", "1 The Street, The Locality, Address 3, The Town, TT1 1TT");
        page.TermShouldExistWithDefinition("Local authority", "Sheffield (321)");
        page.TermShouldExistWithDefinition("Region", "The North");
        page.TermShouldExistWithDefinition("Urban/rural description", "Urban");
    }

    [Fact]
    public async Task SchoolDetailsSection_ContainsSchoolDetails()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x
                .Open().Primary()
                .WithUkPrn("1234567")
                .InLA("123")
                .WithEstablishmentNumber("4567")
                .WithAgeRange(10, 16)
                .WithGender("1", "Mixed")
                .WithTypeOfEstablishment("2", "Multi-academy Trust")
                .WithTrust("1004", "Test Trust")
                .WithAdmissionsPolicy("2", "Non-selective")
                .WithReligiousCharacter("2", "Christian")
                .WithNurseryProvisionName("Has Nursery Classes")
                .WithOfficialSixthForm("1")
                .WithResourcedProvision("4", "SEN unit")
                .WithTrustSchoolFlag("1")
                .WithEstablishmentTypeGroup("4")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        page.TermShouldExistWithDefinition("ID", "URN: 100002, DfE number: 123/4567, UKPRN: 1234567");
        page.TermShouldExistWithDefinition("Age range", "10 to 16");
        page.TermShouldExistWithDefinition("Gender of entry", "Mixed");
        page.TermShouldExistWithDefinition("Phase of education", "Primary");
        page.TermShouldExistWithDefinition("School type", "Multi-academy Trust");
        page.TermShouldExistWithDefinition("Governance structure", "Maintained school - local authority controlled");
        page.TermShouldExistWithDefinition("Academy trust", el => el.ChildElementShouldExist("a")
            .ShouldLinkTo("Test Trust (opens in new tab)", "https://get-information-schools.service.gov.uk/Groups/Group/Details/1004"));
        page.TermShouldExistWithDefinition("Admissions policy", "Non-selective");
        page.TermShouldExistWithDefinition("Religious character", "Christian");
        page.TermShouldExistWithDefinition("Nursery provision", "Has nursery classes");
        page.TermShouldExistWithDefinition("Sixth form", "Has a sixth form");
        page.TermShouldExistWithDefinition("SEN unit", "Has a SEN unit");
        page.TermShouldExistWithDefinition("Resourced provision", "Does not have a resourced provision");
    }

    [Fact]
    public async Task FurtherInformationSection_ContainsCorrectLinks()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x
                .Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        page.TermShouldExistWithDefinition("Ofsted report", el => el.ChildElementShouldExist("a")
            .ShouldLinkTo("View the latest Ofsted report (opens in new tab)", "https://reports.ofsted.gov.uk/provider/21/100002"));
        page.TermShouldExistWithDefinition("Information from other services", el => el.ChildElementsShouldExist("a").Should().SatisfyRespectively(
            a => a.ShouldLinkTo("Financial benchmarking and insights tool (opens in new tab)", "https://financial-benchmarking-and-insights-tool.education.gov.uk/school/100002"),
            a => a.ShouldLinkTo("Get information about schools (opens in new tab)", "https://get-information-schools.service.gov.uk/Establishments/Establishment/Details/100002")));
    }

    [Fact]
    public async Task SchoolDetails_HasSchoolsDataJsonScript()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x
                .Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        var script = page.ElementShouldExist("script#schools-data[type='application/json']");

        var json = script.TrimmedTextContent();
        json.Should().NotBeNullOrWhiteSpace("schools-data script should contain JSON");
        json!.Should().Contain("isMain");
        json.Should().Contain("isComparedSchool");
        json.Should().Contain("lat");
        json.Should().Contain("lon");
    }

    [Fact]
    public async Task SchoolDetails_DisplaysMapDetails_Component()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x
                .Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        var details = page.ElementShouldExist("details#comparison-map-details.govuk-details");

        var summaryText = details.ChildElementShouldExist("summary .govuk-details__summary-text");
        summaryText.TrimmedTextContent().Should().Contain("View on a map");
    }
}
