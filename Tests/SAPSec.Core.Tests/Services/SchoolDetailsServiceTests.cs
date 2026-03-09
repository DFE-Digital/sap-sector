using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model;
using SAPSec.Core.Services;

namespace SAPSec.Core.Tests.Services;

/// <summary>
/// Tests for SchoolDetailsService.
/// Rules are tested through the service - no mocking needed as they are pure functions.
/// </summary>
public class SchoolDetailsServiceTests
{
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly Mock<ILogger<SchoolDetailsService>> _loggerMock;
    private readonly SchoolDetailsService _sut;

    public SchoolDetailsServiceTests()
    {
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        _loggerMock = new Mock<ILogger<SchoolDetailsService>>();

        _sut = new SchoolDetailsService(
            _establishmentRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region GetByUrn Tests

    [Fact]
    public async Task GetByUrn_ValidUrn_ReturnsSchoolDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

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
        var establishment = CreateTestAcademy();
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

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
        var establishment = CreateTestAcademy();
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Address.Value.Should().Contain("Sheffield");
        result.LocalAuthorityName.Value.Should().Be("Sheffield");
        result.LocalAuthorityCode.Value.Should().Be("373");
    }

    [Fact]
    public async Task GetByUrn_SchoolDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("999999"))
            .ReturnsAsync((Establishment?)null);

        // Act
        var action = async () => await _sut.GetByUrnAsync("999999");

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Governance Rule Integration Tests

    [Fact]
    public async Task GetByUrn_AcademyWithTrust_ReturnsMAT()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.MultiAcademyTrust);
        result.AcademyTrustName.Value.Should().Be("Test Trust");
        result.AcademyTrustId.Value.Should().Be("5001");
    }

    [Fact]
    public async Task GetByUrn_AcademyWithoutTrust_ReturnsSAT()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.TrustsId = null;
        establishment.TrustName = null;

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.SingleAcademyTrust);
        result.AcademyTrustName.Availability.Should().Be(DataAvailabilityStatus.NotApplicable);
    }

    [Fact]
    public async Task GetByUrn_LAMaintainedSchool_ReturnsLAMaintained()
    {
        // Arrange
        var establishment = CreateTestLASchool();
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("654321"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("654321");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.LocalAuthorityMaintained);
    }

    [Fact]
    public async Task GetByUrn_IndependentSchool_ReturnsIndependent()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.TypeOfEstablishmentId = "11";
        establishment.TypeOfEstablishmentName = "Other independent school";
        establishment.TrustsId = null;

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.Independent);
    }

    [Fact]
    public async Task GetByUrn_NonMaintainedSpecialSchool_ReturnsCorrectType()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.TypeOfEstablishmentId = "8";
        establishment.TypeOfEstablishmentName = "Non-maintained special school";
        establishment.TrustsId = null;

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.NonMaintainedSpecialSchool);
    }

    #endregion

    #region Nursery Provision Rule Integration Tests

    [Fact]
    public async Task GetByUrn_SecondarySchool_HasNoNurseryProvision()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.PhaseOfEducationName = "Secondary";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasNurseryProvision.Value.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUrn_NurserySchool_HasNurseryProvision()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.PhaseOfEducationName = "Nursery";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasNurseryProvision.Value.Should().BeTrue();
    }

    #endregion

    #region Sixth Form Rule Integration Tests

    [Fact]
    public async Task GetByUrn_SchoolWithSixthForm_HasSixthForm()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.OfficialSixthFormId = "1";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasSixthForm.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUrn_SchoolWithoutSixthForm_DoesNotHaveSixthForm()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.OfficialSixthFormId = "2";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasSixthForm.Value.Should().BeFalse();
    }

    #endregion

    #region SEN/Resourced Provision Rule Integration Tests

    [Fact]
    public async Task GetByUrn_SchoolWithSenUnit_HasSenUnit()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.ResourcedProvisionName = "Has SEN unit";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasSenUnit.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUrn_SchoolWithResourcedProvision_HasResourcedProvision()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.ResourcedProvisionName = "Has resourced provision";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasResourcedProvision.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUrn_SchoolWithBothProvisions_HasBoth()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.ResourcedProvisionName = "Has SEN unit and resourced provision";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasSenUnit.Value.Should().BeTrue();
        result.HasResourcedProvision.Value.Should().BeTrue();
    }

    #endregion

    #region GIAS Special Codes Tests

    [Fact]
    public async Task GetByUrn_RedactedData_ReturnsRedacted()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.GenderName = "c";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.GenderOfEntry.Availability.Should().Be(DataAvailabilityStatus.Redacted);
    }

    [Fact]
    public async Task GetByUrn_NotApplicableData_ReturnsNotApplicable()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.ReligiousCharacterName = "z";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.ReligiousCharacter.Availability.Should().Be(DataAvailabilityStatus.NotApplicable);
    }

    #endregion

    #region Contact Details Tests

    [Fact]
    public async Task GetByUrn_MapsContactDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

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
        var establishment = CreateTestAcademy();
        establishment.Website = "www.testacademy.org.uk";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Website.Value.Should().Be("https://www.testacademy.org.uk");
    }

    #endregion

    #region Test Data Helpers

    private static Establishment CreateTestAcademy()
    {
        return new Establishment
        {
            URN = "123456",
            UKPRN = "10012345",
            EstablishmentName = "Test Academy",
            TypeOfEstablishmentId = "34",
            TypeOfEstablishmentName = "Academy converter",
            TrustsId = "5001",
            TrustName = "Test Trust",
            PhaseOfEducationName = "Secondary",
            OfficialSixthFormId = "1",
            LAName = "Sheffield",
            LAId = "373",
            Street = "123 Test Street",
            Town = "Sheffield",
            Postcode = "S1 1AA",
            GenderName = "Mixed",
            ReligiousCharacterName = "None",
            AdmissionsPolicyName = "Non-selective",
            HeadTitle = "Mr",
            HeadFirstName = "John",
            HeadLastName = "Smith",
            Website = "https://www.testacademy.org.uk",
            TelephoneNum = "0114 123 4567",
            AgeRangeLow = "11",
            AgeRangeRange = "18"
        };
    }

    private static Establishment CreateTestLASchool()
    {
        return new Establishment
        {
            URN = "654321",
            EstablishmentName = "Test Community School",
            TypeOfEstablishmentId = "1",
            TypeOfEstablishmentName = "Community school",
            PhaseOfEducationName = "Primary",
            OfficialSixthFormId = "2",
            LAName = "Sheffield",
            LAId = "373",
            Street = "456 School Lane",
            Town = "Sheffield",
            Postcode = "S2 2BB"
        };
    }

    #endregion
}