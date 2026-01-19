using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Rules;
using SAPSec.Core.Services;
using Xunit;

namespace SAPSec.Core.Tests.Services;

public class SchoolDetailsServiceTests
{
    private readonly Mock<IEstablishmentService> _establishmentServiceMock;
    private readonly Mock<ILogger<SchoolDetailsService>> _loggerMock;
    private readonly SchoolDetailsService _sut;

    // Use real rules for integration-style tests
    private readonly GovernanceRule _governanceRule;
    private readonly NurseryProvisionRule _nurseryProvisionRule;
    private readonly SixthFormRule _sixthFormRule;
    private readonly SenUnitRule _senUnitRule;
    private readonly ResourcedProvisionRule _resourcedProvisionRule;

    public SchoolDetailsServiceTests()
    {
        _establishmentServiceMock = new Mock<IEstablishmentService>();
        _loggerMock = new Mock<ILogger<SchoolDetailsService>>();

        // Use real rule implementations
        _governanceRule = new GovernanceRule();
        _nurseryProvisionRule = new NurseryProvisionRule();
        _sixthFormRule = new SixthFormRule();
        _senUnitRule = new SenUnitRule();
        _resourcedProvisionRule = new ResourcedProvisionRule();

        _sut = new SchoolDetailsService(
            _establishmentServiceMock.Object,
            _governanceRule,
            _nurseryProvisionRule,
            _sixthFormRule,
            _senUnitRule,
            _resourcedProvisionRule,
            _loggerMock.Object);
    }

    #region GetByUrn Tests

    [Fact]
    public void GetByUrn_ValidUrn_ReturnsSchoolDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.Should().NotBeNull();
        result.Urn.Value.Should().Be("123456");
        result.Name.Value.Should().Be("Test Academy");
    }

    [Fact]
    public void GetByUrn_ValidUrn_MapsAllIdentifiers()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.Urn.Value.Should().Be("123456");
        result.Ukprn.Value.Should().Be("10012345");
    }

    [Fact]
    public void GetByUrn_ValidUrn_MapsLocationFields()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.Address.Value.Should().Contain("Sheffield");
        result.LocalAuthorityName.Value.Should().Be("Sheffield");
        result.LocalAuthorityCode.Value.Should().Be("373");
    }

    [Fact]
    public void GetByUrn_AcademyWithTrust_ReturnsMAT()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.MultiAcademyTrust);
        result.AcademyTrustName.Value.Should().Be("Test Trust");
        result.AcademyTrustId.Value.Should().Be("5001");
    }

    [Fact]
    public void GetByUrn_AcademyWithoutTrust_ReturnsSAT()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.TrustsId = null;
        establishment.TrustName = null;

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.SingleAcademyTrust);
        result.AcademyTrustName.Availability.Should().Be(DataAvailability.NotApplicable);
    }

    [Fact]
    public void GetByUrn_LAMaintainedSchool_ReturnsLAMaintained()
    {
        // Arrange
        var establishment = CreateTestLASchool();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("654321"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("654321");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.LocalAuthorityMaintained);
    }

    [Fact]
    public void GetByUrn_SecondarySchool_HasNoNurseryProvision()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.PhaseOfEducationName = "Secondary";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.HasNurseryProvision.Value.Should().BeFalse();
    }

    [Fact]
    public void GetByUrn_NurserySchool_HasNurseryProvision()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.PhaseOfEducationName = "Nursery";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.HasNurseryProvision.Value.Should().BeTrue();
    }

    [Fact]
    public void GetByUrn_SchoolWithSixthForm_HasSixthForm()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.OfficialSixthFormId = "1";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.HasSixthForm.Value.Should().BeTrue();
    }

    [Fact]
    public void GetByUrn_SchoolWithoutSixthForm_DoesNotHaveSixthForm()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.OfficialSixthFormId = "2";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.HasSixthForm.Value.Should().BeFalse();
    }

    [Fact]
    public void GetByUrn_SchoolWithSenUnit_HasSenUnit()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.ResourcedProvision = "Has SEN unit";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.HasSenUnit.Value.Should().BeTrue();
    }

    [Fact]
    public void GetByUrn_SchoolWithResourcedProvision_HasResourcedProvision()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.ResourcedProvision = "Has resourced provision";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.HasResourcedProvision.Value.Should().BeTrue();
    }

    [Fact]
    public void GetByUrn_SchoolWithBothProvisions_HasBoth()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.ResourcedProvision = "Has SEN unit and resourced provision";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.HasSenUnit.Value.Should().BeTrue();
        result.HasResourcedProvision.Value.Should().BeTrue();
    }

    [Fact]
    public void GetByUrn_MapsContactDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.HeadteacherName.Value.Should().Be("Mr John Smith");
        result.Website.Value.Should().Be("https://www.testacademy.org.uk");
        result.Telephone.Value.Should().Be("0114 123 4567");
    }

    [Fact]
    public void GetByUrn_WebsiteWithoutProtocol_AddsHttps()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.Website = "www.testacademy.org.uk";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.Website.Value.Should().Be("https://www.testacademy.org.uk");
    }

    [Fact]
    public void GetByUrn_MissingData_ReturnsNotAvailable()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Minimal School"
        };

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.Name.Value.Should().Be("Minimal School");
        result.Address.Availability.Should().Be(DataAvailability.NotAvailable);
        result.LocalAuthorityName.Availability.Should().Be(DataAvailability.NotAvailable);
        result.HasNurseryProvision.Availability.Should().Be(DataAvailability.NotAvailable);
        result.HasSixthForm.Availability.Should().Be(DataAvailability.NotAvailable);
    }

    #endregion

    #region TryGetByUrn Tests

    [Fact]
    public void TryGetByUrn_ValidUrn_ReturnsSchoolDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.TryGetByUrn("123456");

        // Assert
        result.Should().NotBeNull();
        result!.Urn.Value.Should().Be("123456");
    }

    [Fact]
    public void TryGetByUrn_ServiceThrowsException_ReturnsNull()
    {
        // Arrange
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("999999"))
            .Throws(new KeyNotFoundException("School not found"));

        // Act
        var result = _sut.TryGetByUrn("999999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryGetByUrn_ServiceThrowsException_LogsDebug()
    {
        // Arrange
        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("999999"))
            .Throws(new KeyNotFoundException("School not found"));

        // Act
        _sut.TryGetByUrn("999999");

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
    public void GetByIdentifier_ValidIdentifier_ReturnsSchoolDetails()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentByAnyNumber("373/1234"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByIdentifier("373/1234");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Value.Should().Be("Test Academy");
    }

    [Fact]
    public void GetByIdentifier_NotFound_ReturnsNull()
    {
        // Arrange
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentByAnyNumber("invalid"))
            .Returns(new Establishment());

        // Act
        var result = _sut.GetByIdentifier("invalid");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetByIdentifier_NullUrn_ReturnsNull()
    {
        // Arrange
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentByAnyNumber("invalid"))
            .Returns(new Establishment { URN = null });

        // Act
        var result = _sut.GetByIdentifier("invalid");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetByIdentifier_EmptyUrn_ReturnsNull()
    {
        // Arrange
        _establishmentServiceMock
            .Setup(x => x.GetEstablishmentByAnyNumber("invalid"))
            .Returns(new Establishment { URN = "" });

        // Act
        var result = _sut.GetByIdentifier("invalid");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GIAS Special Codes Tests

    [Fact]
    public void GetByUrn_RedactedData_ReturnsRedacted()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.GenderName = "c";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.GenderOfEntry.Availability.Should().Be(DataAvailability.Redacted);
    }

    [Fact]
    public void GetByUrn_NotApplicableData_ReturnsNotApplicable()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.ReligiousCharacterName = "z";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.ReligiousCharacter.Availability.Should().Be(DataAvailability.NotApplicable);
    }

    [Fact]
    public void GetByUrn_NotAvailableData_ReturnsNotAvailable()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.AdmissionPolicy = "x";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.AdmissionsPolicy.Availability.Should().Be(DataAvailability.NotAvailable);
    }

    #endregion

    #region Age Range Tests

    [Fact]
    public void GetByUrn_ValidAgeRange_MapsCorrectly()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.AgeRangeLow = "11";
        establishment.AgeRangeRange = "18";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.AgeRangeLow.Value.Should().Be(11);
        result.AgeRangeHigh.Value.Should().Be(18);
    }

    [Fact]
    public void GetByUrn_InvalidAgeRange_ReturnsNotAvailable()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.AgeRangeLow = "abc";
        establishment.AgeRangeRange = null;

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.AgeRangeLow.Availability.Should().Be(DataAvailability.NotAvailable);
        result.AgeRangeHigh.Availability.Should().Be(DataAvailability.NotAvailable);
    }

    #endregion

    #region Establishment Type Tests

    [Theory]
    [InlineData("28", GovernanceType.MultiAcademyTrust)]  // Academy sponsor led
    [InlineData("34", GovernanceType.MultiAcademyTrust)]  // Academy converter
    [InlineData("35", GovernanceType.MultiAcademyTrust)]  // Free school
    public void GetByUrn_AcademyTypesWithTrust_ReturnsMAT(string typeId, GovernanceType expected)
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.TypeOfEstablishmentId = typeId;
        establishment.TrustsId = "5001";

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("1")]   // Community school
    [InlineData("2")]   // Voluntary aided
    [InlineData("3")]   // Voluntary controlled
    [InlineData("5")]   // Foundation
    public void GetByUrn_LAMaintainedTypes_ReturnsLAMaintained(string typeId)
    {
        // Arrange
        var establishment = CreateTestLASchool();
        establishment.TypeOfEstablishmentId = typeId;

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("654321"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("654321");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.LocalAuthorityMaintained);
    }

    [Fact]
    public void GetByUrn_IndependentSchool_ReturnsIndependent()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.TypeOfEstablishmentId = "11";
        establishment.TypeOfEstablishmentName = "Other independent school"; // Must clear academy name
        establishment.TrustsId = null;

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.Independent);
    }

    [Fact]
    public void GetByUrn_NonMaintainedSpecialSchool_ReturnsCorrectType()
    {
        // Arrange
        var establishment = CreateTestAcademy();
        establishment.TypeOfEstablishmentId = "8";
        establishment.TypeOfEstablishmentName = "Non-maintained special school"; // Must clear academy name
        establishment.TrustsId = null;

        _establishmentServiceMock
            .Setup(x => x.GetEstablishment("123456"))
            .Returns(establishment);

        // Act
        var result = _sut.GetByUrn("123456");

        // Assert
        result.GovernanceStructure.Value.Should().Be(GovernanceType.NonMaintainedSpecialSchool);
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
            LANAme = "Sheffield",
            LAId = "373",
            AddressStreet = "123 Test Street",
            AddressTown = "Sheffield",
            AddressPostcode = "S1 1AA",
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
            LANAme = "Sheffield",
            LAId = "373",
            AddressStreet = "456 School Lane",
            AddressTown = "Sheffield",
            AddressPostcode = "S2 2BB"
        };
    }

    #endregion
}