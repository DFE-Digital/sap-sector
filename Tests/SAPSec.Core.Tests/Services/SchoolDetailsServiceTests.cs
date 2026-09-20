using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Model;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Services;

/// <summary>
/// Tests for SchoolDetailsService.
/// Rules are tested through the service - no mocking needed as they are pure functions.
/// </summary>
public class SchoolDetailsServiceTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepository;
    private readonly Mock<ILogger<SchoolDetailsService>> _loggerMock;
    private readonly SchoolDetailsService _sut;

    public SchoolDetailsServiceTests()
    {
        _establishmentRepository = new InMemoryEstablishmentRepository();
        _loggerMock = new Mock<ILogger<SchoolDetailsService>>();

        _sut = new SchoolDetailsService(
            _establishmentRepository,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByUrn_ValidUrn_ReturnsSchoolDetails()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy"));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Should().NotBeNull();
        result.Urn.Should().Be("123456");
        result.Name.Should().Be("Test Academy");
    }

    [Fact]
    public async Task GetByUrn_ValidUrn_MapsAllIdentifiers()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x.WithUkPrn("10012345")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Urn.Should().Be("123456");
        result.Ukprn.Value.Should().Be("10012345");
    }

    [Fact]
    public async Task GetByUrn_ValidUrn_MapsLocationFields()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .InLA("373", "Sheffield")
                .WithAddress(
                    "123 Test Street",
                    "Someplace",
                    "Somewhere",
                    "Sheffield",
                    "S1 1AA")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Address.Value.Should().Be("123 Test Street, Someplace, Somewhere, Sheffield, S1 1AA");
        result.LocalAuthorityName.Value.Should().Be("Sheffield");
        result.LocalAuthorityCode.Value.Should().Be("373");
    }

    [Fact]
    public async Task GetByUrn_SchoolDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange

        // Act
        var action = async () => await _sut.GetByUrnAsync("999999");

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByUrn_AdmissionsPolicy_ShouldNotBeANumber()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithAdmissionsPolicy("2", "Non-selective")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");
        var isNumeric = int.TryParse(result.AdmissionsPolicy.Value, out _);

        // Assert
        isNumeric.Should().BeFalse("Admissions policy value should not be a number");
        result.AdmissionsPolicy.Value.Should().Be("Non-selective");
    }

    [Fact]
    public async Task GetByUrn_AcademyWithTrust_ReturnsMAT()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithTrust("5001", "Test Trust")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.MultiAcademyTrust);
        result.AcademyTrustName.Value.Should().Be("Test Trust");
        result.AcademyTrustId.Value.Should().Be("5001");
    }

    [Fact]
    public async Task GetByUrn_ReturnsSAT()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithTrustSchoolFlag("5")
                .WithEstablishmentTypeGroup("10")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.SingleAcademyTrust);
    }

    [Fact]
    public async Task GetByUrn_LAMaintainedSchool_ReturnsLAMaintained()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("654321", "Test Academy", x => x
                .WithTrustSchoolFlag("1")
                .WithEstablishmentTypeGroup("4")));
        //var establishment = CreateTestLASchool();

        // Act
        var result = await _sut.GetByUrnAsync("654321");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.LocalAuthorityMaintained);
    }

    [Fact]
    public async Task GetByUrn_SecondarySchool_HasNoNurseryProvision()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithNurseryProvisionName("No Nursery Classes")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasNurseryProvision.Value.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUrn_NurserySchool_HasNurseryProvision()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithNurseryProvisionName("Has Nursery Classes")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasNurseryProvision.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUrn_SchoolWithSixthForm_HasSixthForm()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithOfficialSixthForm("1")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasSixthForm.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUrn_SchoolWithoutSixthForm_DoesNotHaveSixthForm()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithOfficialSixthForm("2")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasSixthForm.Value.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUrn_SchoolWithSenUnit_HasSenUnit()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithResourcedProvision("XXX", "SEN unit")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasSenUnit.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUrn_SchoolWithResourcedProvision_HasResourcedProvision()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithResourcedProvision("XXX", "Has resourced provision")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasResourcedProvision.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUrn_SchoolWithNoJustASENUnit_DoesNotHaveResourcedProvision()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithResourcedProvision("XXX", "SEN unit")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasResourcedProvision.Value.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUrn_RedactedData_ReturnsRedacted()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithGender("c", "c")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.GenderOfEntry.Availability.Should().Be(DataAvailabilityStatus.Redacted);
    }

    [Fact]
    public async Task GetByUrn_NotApplicableData_ReturnsNotApplicable()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithReligiousCharacter("z", "z")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.ReligiousCharacter.Availability.Should().Be(DataAvailabilityStatus.NotApplicable);
    }

    [Fact]
    public async Task GetByUrn_MapsContactDetails()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithHeadTeacher("Mr", "John", "Smith")
                .WithWebsite("https://www.testacademy.org.uk")
                .WithTelephone("0114 123 4567")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HeadteacherName.Value.Should().Be("Mr John Smith");
        result.Website.Value.Should().Be("https://www.testacademy.org.uk");
        result.Telephone.Value.Should().Be("0114 123 4567");
    }

    [Fact]
    public async Task GetByUrn_WebsiteWithoutProtocol_AddsHttps()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy", x => x
                .WithWebsite("www.testacademy.org.uk")));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Website.Value.Should().Be("https://www.testacademy.org.uk");
    }

    [Fact]
    public async Task GetByUrn_MapsEmailAddress()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy"))
            .SetupEstablishmentEmails(Build.EstablishmentEmail("123456", "establishment@email.com"));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Email.Value.Should().Be("establishment@email.com");
    }

    [Fact]
    public async Task GetByUrn_MapsMissingEmailAddressToNotAvailable()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy"));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Email.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    [Fact]
    public async Task GetByUrn_MapsEmptyEmailAddressToNotAvailable()
    {
        // Arrange
        _establishmentRepository
            .SetupEstablishments(Build.Establishment("123456", "Test Academy"))
            .SetupEstablishmentEmails(Build.EstablishmentEmail("123456", ""));

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Email.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }
}