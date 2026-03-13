using FluentAssertions;

namespace SAPSec.Integration.Tests;

public class SchoolControllerIntegrationTests
{
    private static readonly string SchoolIndexViewPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "SAPSec.Web", "Views", "School", "Index.cshtml"));

    private static readonly string SchoolDetailsViewPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "SAPSec.Web", "Views", "School", "SchoolDetails.cshtml"));

    private static readonly string SchoolLayoutViewPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "SAPSec.Web", "Views", "School", "_Layout.cshtml"));

    [Fact]
    public async Task GetSchool_OverviewViewExists()
    {
        File.Exists(SchoolIndexViewPath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(SchoolIndexViewPath);

        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetSchool_OverviewContainsExpectedContent()
    {
        var content = await File.ReadAllTextAsync(SchoolIndexViewPath);

        content.Should().Contain("Compare school performance");
        content.Should().Contain("View similar schools to connect with");
        content.Should().Contain("how the DfE defines what a similar school is");
    }

    [Fact]
    public async Task GetSchool_SchoolDetailsViewExists()
    {
        File.Exists(SchoolDetailsViewPath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(SchoolDetailsViewPath);

        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetSchool_ContainsLocationSection()
    {
        var content = await File.ReadAllTextAsync(SchoolDetailsViewPath);

        content.Should().Contain("Location");
        content.Should().Contain("Address");
        content.Should().Contain("Local authority");
        content.Should().Contain("Region");
        content.Should().Contain("Urban/rural description");
    }

    [Fact]
    public async Task GetSchool_ContainsSchoolDetailsSection()
    {
        var content = await File.ReadAllTextAsync(SchoolDetailsViewPath);

        content.Should().Contain("School details");
        content.Should().Contain("ID");
        content.Should().Contain("Age range");
        content.Should().Contain("Gender of entry");
        content.Should().Contain("Phase of education");
        content.Should().Contain("School type");
        content.Should().Contain("Governance structure");
        content.Should().Contain("Academy trust");
        content.Should().Contain("Admissions policy");
        content.Should().Contain("Resourced provision");
        content.Should().Contain("Religious character");
    }

    [Fact]
    public async Task GetSchool_ContainsContactDetailsSection()
    {
        var content = await File.ReadAllTextAsync(SchoolDetailsViewPath);

        content.Should().Contain("Contact details");
        content.Should().Contain("Headteacher / Principal");
        content.Should().Contain("Website");
        content.Should().Contain("Telephone");
        content.Should().Contain("Email");
    }

    [Fact]
    public async Task GetSchool_ContainsFurtherInformationSection()
    {
        var content = await File.ReadAllTextAsync(SchoolDetailsViewPath);

        content.Should().Contain("Further information");
        content.Should().Contain("Ofsted report");
        content.Should().Contain("reports.ofsted.gov.uk");
    }

    [Fact]
    public async Task GetSchool_HasExpectedStructureAndLinks()
    {
        var content = await File.ReadAllTextAsync(SchoolDetailsViewPath);

        content.Should().Contain("ViewData[\"Title\"] = $\"School details\"");
        content.Should().Contain("School details");
        content.Should().Contain("govuk-summary-list");
        content.Should().Contain("govuk-summary-list__key");
        content.Should().Contain("govuk-summary-list__value");
        content.Should().Contain("govuk-heading-xl");
        content.Should().Contain("govuk-heading-m");
        content.Should().Contain("target=\"_blank\"");
        content.Should().Contain("rel=\"noopener noreferrer\"");
        content.Should().Contain("opens in a new tab");
    }

    [Fact]
    public async Task GetSchool_LayoutContainsNavigationLinks()
    {
        File.Exists(SchoolLayoutViewPath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(SchoolLayoutViewPath);

        content.Should().Contain("Overview");
        content.Should().Contain("KS4 headline measures");
        content.Should().Contain("KS4 core subjects");
        content.Should().Contain("Attendance");
        content.Should().Contain("View similar schools");
        content.Should().Contain("School details");
        content.Should().Contain("What is a similar school?");
        content.Should().Contain("govuk-breadcrumbs__link");
    }
}
