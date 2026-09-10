using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

public class SchoolSchoolDetailsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task NonExistentUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Open().Secondary()));

        await Fixture.RequestPageAsync(Routes.SecondarySchool("999999").SchoolDetails, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CaptionAndHeading_DisplaysSchoolName()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").SchoolDetails, HttpStatusCode.OK);

        var heading = page.ElementShouldExist("h1.govuk-heading-xl");
        heading.TrimmedTextContent().Should().Be("School details");

        var caption = page.ElementShouldExist("span.govuk-caption-xl");
        caption.TrimmedTextContent().Should().Be("Test School");
    }

    [Fact]
    public async Task ContactDetailsSection_ContainsSchoolContactDetails()
    {
        Fixture.EstablishmentRepository
            .SetupEstablishments(
                Build.Establishment("100001", "Test School", x => x
                    .Open().Secondary()
                    .WithHeadTeacher("Miss", "Jane", "Smith")
                    .WithTelephone("01234 567890")
                    .WithWebsite("https://similar-school.example.com")))
            .SetupEstablishmentEmails(
                Build.EstablishmentEmail("100001", "test@education.gov.uk"));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").SchoolDetails, HttpStatusCode.OK);

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
            Build.Establishment("100001", "Test School", x => x
                .Open().Secondary()
                .WithAddress("1 The Street", "The Locality", "Address 3", "The Town", "TT1 1TT")
                .InLA("321", "Sheffield")
                .InAdministrativeDistrict("1", "The North")
                .WithUrbanRural("2", "Urban")));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").SchoolDetails, HttpStatusCode.OK);

        page.TermShouldExistWithDefinition("Address", "1 The Street, The Locality, Address 3, The Town, TT1 1TT");
        page.TermShouldExistWithDefinition("Local authority", "Sheffield (321)");
        page.TermShouldExistWithDefinition("Region", "The North");
        page.TermShouldExistWithDefinition("Urban/rural description", "Urban");
    }

    [Fact]
    public async Task SchoolDetailsSection_ContainsSchoolDetails()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x
                .Open().Secondary()
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

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").SchoolDetails, HttpStatusCode.OK);

        page.TermShouldExistWithDefinition("ID", "URN: 100001, DfE number: 123/4567, UKPRN: 1234567");
        page.TermShouldExistWithDefinition("Age range", "10 to 16");
        page.TermShouldExistWithDefinition("Gender of entry", "Mixed");
        page.TermShouldExistWithDefinition("Phase of education", "Secondary");
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
            Build.Establishment("100001", "Test School", x => x
                .Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").SchoolDetails, HttpStatusCode.OK);

        page.TermShouldExistWithDefinition("Ofsted report", el => el.ChildElementShouldExist("a")
            .ShouldLinkTo("View the latest Ofsted report (opens in new tab)", "https://reports.ofsted.gov.uk/provider/23/100001"));
        page.TermShouldExistWithDefinition("Information from other services", el => el.ChildElementsShouldExist("a").Should().SatisfyRespectively(
            a => a.ShouldLinkTo("Financial benchmarking and insights tool (opens in new tab)", "https://financial-benchmarking-and-insights-tool.education.gov.uk/school/100001"),
            a => a.ShouldLinkTo("Get information about schools (opens in new tab)", "https://get-information-schools.service.gov.uk/Establishments/Establishment/Details/100001")));
    }
}
