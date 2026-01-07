using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;


namespace SAPSec.Web.Tests.Controllers;

public class SchoolControllerTests
{
    private readonly Mock<ILogger<SchoolController>> _mockLogger;
    private readonly Mock<IEstablishmentService> _mockEstablishmentService;
    private readonly SchoolController _controller;

    private static readonly Establishment FakeEstablishment = new()
    {
        URN = "147788",
        UKPRN = "10085445",
        LAId = "373",
        LANAme = "Sheffield",
        EstablishmentNumber = "4017",
        EstablishmentName = "Bradfield School",
        AddressStreet = "Kirk Edge Road",
        AddressLocality = "",
        AddressAddress3 = "",
        AddressTown = "Sheffield",
        AddressPostcode = "S35 0AE",
        HeadteacherTitle = "Mr",
        HeadteacherFirstName = "Dale",
        HeadteacherLastName = "Barrowclough",
        HeadteacherPreferredJobTitle = "Headteacher",
        TelephoneNum = "01142863861",
        Website = "www.bradfield.sheffield.sch.uk",
        TrustsId = "4899",
        TrustName = "Sheffield South East Trust",
        TypeOfEstablishmentName = "Academy converter",
        PhaseOfEducationName = "Secondary",
        GenderName = "Mixed",
        ReligiousCharacterName = "None",
        AdmissionPolicy = "Non-selective",
        ResourcedProvision = "Resourced provision",
        DistrictAdministrativeName = "Yorkshire and the Humber",
        UrbanRuralName = "Larger rural: Nearer to a major town or city"
    };

    public SchoolControllerTests()
    {
        _mockLogger = new Mock<ILogger<SchoolController>>();
        _mockEstablishmentService = new Mock<IEstablishmentService>();
        _controller = new SchoolController(_mockEstablishmentService.Object, _mockLogger.Object);
    }

    #region Index GET Tests

    [Fact]
    public void Index_WithValidUrn_ReturnsViewResult()
    {
        // Arrange
        var urn = "147788";
        _mockEstablishmentService.Setup(s => s.GetEstablishment(urn))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index(urn);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Index_WithValidUrn_ReturnsViewWithSchoolViewModel()
    {
        // Arrange
        var urn = "147788";
        _mockEstablishmentService.Setup(s => s.GetEstablishment(urn))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index(urn) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result.Model.Should().BeOfType<SchoolViewModel>();
    }

    [Fact]
    public void Index_WithValidUrn_ReturnsCorrectSchoolName()
    {
        // Arrange
        var urn = "147788";
        _mockEstablishmentService.Setup(s => s.GetEstablishment(urn))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index(urn) as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Name.Should().Be("Bradfield School");
    }

    [Fact]
    public void Index_WithValidUrn_ReturnsCorrectUrn()
    {
        // Arrange
        var urn = "147788";
        _mockEstablishmentService.Setup(s => s.GetEstablishment(urn))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index(urn) as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Urn.Should().Be("147788");
    }
    [Fact]
    public void Index_CallsEstablishmentServiceWithCorrectUrn()
    {
        // Arrange
        var urn = "147788";
        _mockEstablishmentService.Setup(s => s.GetEstablishment(urn))
            .Returns(FakeEstablishment);

        // Act
        _controller.Index(urn);

        // Assert
        _mockEstablishmentService.Verify(s => s.GetEstablishment(urn), Times.Once);
    }

    #endregion

    #region ViewModel Mapping Tests - Location Section

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectAddress()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Address.Should().Contain("Kirk Edge Road");
        model.Address.Should().Contain("Sheffield");
        model.Address.Should().Contain("S35 0AE");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectLocalAuthority()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.LocalAuthority.Should().Contain("Sheffield");
        model.LocalAuthority.Should().Contain("373");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectRegion()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Region.Should().Be("Yorkshire and the Humber");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectUrbanRuralDescription()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.UrbanRuralDescription.Should().Be("Larger rural: Nearer to a major town or city");
    }

    #endregion

    #region ViewModel Mapping Tests - School Details Section

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectId()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Id.Should().Contain("URN: 147788");
        model.Id.Should().Contain("DfE number: 373/4017");
        model.Id.Should().Contain("UKPRN: 10085445");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectGenderOfEntry()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.GenderOfEntry.Should().Be("Mixed");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectPhaseOfEducation()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.PhaseOfEducation.Should().Be("Secondary");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectSchoolType()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.SchoolType.Should().Be("Academy converter");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectGovernanceStructure_WhenPartOfTrust()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.GovernanceStructure.Should().Be("Multi-academy trust (MAT)");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectAcademyTrust()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.AcademyTrust.Should().Be("Sheffield South East Trust");
        model.IsPartOfTrust.Should().BeTrue();
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectAdmissionsPolicy()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.AdmissionsPolicy.Should().Be("Non-selective");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectSendIntegratedResource()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.SendIntegratedResource.Should().Be("Resourced provision");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectReligiousCharacter()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.ReligiousCharacter.Should().Be("None");
    }

    #endregion

    #region ViewModel Mapping Tests - Contact Details Section

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectHeadteacher()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.HeadteacherPrincipal.Should().Contain("Mr");
        model.HeadteacherPrincipal.Should().Contain("Dale");
        model.HeadteacherPrincipal.Should().Contain("Barrowclough");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectWebsite()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.HasWebsite.Should().BeTrue();
        model.WebsiteUrl.Should().Contain("bradfield.sheffield.sch.uk");
    }

    [Fact]
    public void Index_ReturnsViewModel_WithCorrectTelephone()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(FakeEstablishment);

        // Act
        var result = _controller.Index("147788") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Telephone.Should().Be("01142863861");
    }

    #endregion

    #region No Data Available Tests

    [Fact]
    public void Index_ReturnsNoAvailableData_WhenAddressIsEmpty()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test School"
        };
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(establishment);

        // Act
        var result = _controller.Index("123456") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Address.Should().Be("No available data");
    }

    [Fact]
    public void Index_ReturnsNoAvailableData_WhenLocalAuthorityIsEmpty()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test School",
            LANAme = ""
        };
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(establishment);

        // Act
        var result = _controller.Index("123456") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.LocalAuthority.Should().Be("No available data");
    }

    [Fact]
    public void Index_ReturnsNoAvailableData_WhenRegionIsEmpty()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test School",
            DistrictAdministrativeName = ""
        };
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(establishment);

        // Act
        var result = _controller.Index("123456") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Region.Should().Be("No available data");
    }

    [Fact]
    public void Index_ReturnsNoAvailableData_WhenHeadteacherIsEmpty()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test School"
        };
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(establishment);

        // Act
        var result = _controller.Index("123456") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.HeadteacherPrincipal.Should().Be("No available data");
    }

    [Fact]
    public void Index_ReturnsNoAvailableData_WhenWebsiteIsEmpty()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test School",
            Website = ""
        };
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(establishment);

        // Act
        var result = _controller.Index("123456") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.Website.Should().Be("No available data");
        model.HasWebsite.Should().BeFalse();
    }

    [Fact]
    public void Index_ReturnsNoAvailableData_WhenNotPartOfTrust()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test School",
            TrustsId = "",
            TrustName = ""
        };
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(establishment);

        // Act
        var result = _controller.Index("123456") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.AcademyTrust.Should().Be("No available data");
        model.IsPartOfTrust.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Index_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Service error"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _controller.Index("147788"));
    }

    [Fact]
    public void Index_WithSpecialCharactersInUrn_CallsServiceCorrectly()
    {
        // Arrange
        var urn = "123-456";
        _mockEstablishmentService.Setup(s => s.GetEstablishment(urn))
            .Returns((Establishment?)null);

        // Act
        var result = _controller.Index(urn);

        // Assert
        _mockEstablishmentService.Verify(s => s.GetEstablishment(urn), Times.Once);
    }

    [Fact]
    public void Index_WebsiteUrl_AddsHttpsIfMissing()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test School",
            Website = "www.example.com"
        };
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(establishment);

        // Act
        var result = _controller.Index("123456") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.WebsiteUrl.Should().StartWith("https://");
    }

    [Fact]
    public void Index_WebsiteUrl_DoesNotAddHttpsIfAlreadyPresent()
    {
        // Arrange
        var establishment = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test School",
            Website = "https://www.example.com"
        };
        _mockEstablishmentService.Setup(s => s.GetEstablishment(It.IsAny<string>()))
            .Returns(establishment);

        // Act
        var result = _controller.Index("123456") as ViewResult;
        var model = result!.Model as SchoolViewModel;

        // Assert
        model!.WebsiteUrl.Should().Be("https://www.example.com");
        model.WebsiteUrl.Should().NotContain("https://https://");
    }

    #endregion
}