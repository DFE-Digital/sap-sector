using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Services;
using Xunit;

namespace SAPSec.Core.Tests.Services;

/// <summary>
/// Tests for SchoolDetailsService.
/// Rules are tested through the service - no mocking needed as they are pure functions.
/// </summary>
public class SchoolDetailsServiceTests
{
    private readonly Mock<IEstablishmentService> _establishmentServiceMock;
    private readonly Mock<ILogger<SchoolDetailsService>> _loggerMock;
    private readonly SchoolDetailsService _sut;

    public SchoolDetailsServiceTests()
    {
        _establishmentServiceMock = new Mock<IEstablishmentService>();
        _loggerMock = new Mock<ILogger<SchoolDetailsService>>();

        _sut = new SchoolDetailsService(
            _establishmentServiceMock.Object,
            _loggerMock.Object);
    }

    #region GetByUrn Tests

    [Fact]
    public async Task GetByUrn_ValidUrn_ReturnsSchoolDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Should().NotBeNull();
        result.Urn.Value.Should().Be("123456");
        result.Name.Value.Should().Be("Test Academy");
    }

    [Fact]
    public async Task GetByUrn_ValidUrn_MapsAllIdentifiers()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Urn.Value.Should().Be("123456");
        result.Ukprn.Value.Should().Be("10012345");
    }

    [Fact]
    public async Task GetByUrn_ValidUrn_MapsLocationFields()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.Address.Value.Should().Contain("Sheffield");
        result.LocalAuthorityName.Value.Should().Be("Sheffield");
        result.LocalAuthorityCode.Value.Should().Be("373");
    }

    #endregion

    #region Governance Rule Integration Tests

    [Fact]
    public async Task GetByUrn_AcademyWithTrust_ReturnsMAT()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
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

        _establishmentServiceMock
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
        _establishmentServiceMock
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

        _establishmentServiceMock
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

        _establishmentServiceMock
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

        _establishmentServiceMock
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

        _establishmentServiceMock
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

        _establishmentServiceMock
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

        _establishmentServiceMock
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
        establishment.ResourcedProvision = "Has SEN unit";

        _establishmentServiceMock
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
        establishment.ResourcedProvision = "Has resourced provision";

        _establishmentServiceMock
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
        establishment.ResourcedProvision = "Has SEN unit and resourced provision";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByUrnAsync("123456");

        // Assert
        result.HasSenUnit.Value.Should().BeTrue();
        result.HasResourcedProvision.Value.Should().BeTrue();
    }

    #endregion

    #region TryGetByUrn Tests

    [Fact]
    public async Task TryGetByUrn_ValidUrn_ReturnsSchoolDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.TryGetByUrnAsync("123456");

        // Assert
        result.Should().NotBeNull();
        result!.Urn.Value.Should().Be("123456");
    }

    [Fact]
    public async Task TryGetByUrn_ServiceThrowsException_ReturnsNull()
    {
        // Arrange
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentAsync("999999"))
            .Throws(new KeyNotFoundException("School not found"));

        // Act
        var result = await _sut.TryGetByUrnAsync("999999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TryGetByUrn_ServiceThrowsException_LogsDebug()
    {
        // Arrange
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentAsync("999999"))
            .Throws(new KeyNotFoundException("School not found"));

        // Act
        await _sut.TryGetByUrnAsync("999999");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("999999")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetByIdentifier Tests

    [Fact]
    public async Task GetByIdentifier_ValidIdentifier_ReturnsSchoolDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("373/1234"))
            .ReturnsAsync(establishment);

        // Act
        var result = await _sut.GetByIdentifierAsync("373/1234");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Value.Should().Be("Test Academy");
    }

    [Fact]
    public async Task GetByIdentifier_NullUrn_ReturnsNull()
    {
        // Arrange
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("invalid"))
            .ReturnsAsync(new Establishment { URN = null });

        // Act
        var result = await _sut.GetByIdentifierAsync("invalid");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GIAS Special Codes Tests

    [Fact]
    public async Task GetByUrn_RedactedData_ReturnsRedacted()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.GenderName = "c";

        _establishmentServiceMock
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

        _establishmentServiceMock
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
        _establishmentServiceMock
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

        _establishmentServiceMock
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
            AdmissionPolicy = "Non-selective",
            HeadteacherTitle = "Mr",
            HeadteacherFirstName = "John",
            HeadteacherLastName = "Smith",
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