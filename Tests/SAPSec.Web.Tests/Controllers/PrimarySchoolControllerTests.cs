using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Areas.Primary.Controllers;

namespace SAPSec.Web.Tests.Controllers;

public class PrimarySchoolControllerTests
{
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock = new();
    private readonly Mock<IRequestSchoolAccessor> _requestSchoolAccessorMock = new();

    [Fact]
    public async Task Index_WhenFeatureDisabled_ReturnsNotFound()
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync("EnablePrimarySchools"))
            .ReturnsAsync(false);

        var sut = new SchoolController(_featureFlagServiceMock.Object, _requestSchoolAccessorMock.Object);

        var result = await sut.Index("123456");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenFeatureEnabled_ReturnsViewWithSchoolDetails()
    {
        var school = CreateTestSchoolDetails("123456", "Test Primary School");
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync("EnablePrimarySchools"))
            .ReturnsAsync(true);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext>(), "123456"))
            .ReturnsAsync(school);

        var sut = new SchoolController(_featureFlagServiceMock.Object, _requestSchoolAccessorMock.Object);

        var result = await sut.Index("123456");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(school);
        sut.ViewData["SchoolDetails"].Should().Be(school);
        sut.ViewData["BreadcrumbNode"].Should().NotBeNull();
    }

    [Fact]
    public async Task WhatIsASimilarSchool_WhenFeatureEnabled_ReturnsViewWithSchoolDetails()
    {
        var school = CreateTestSchoolDetails("123456", "Test Primary School");
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync("EnablePrimarySchools"))
            .ReturnsAsync(true);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext>(), "123456"))
            .ReturnsAsync(school);

        var sut = new SchoolController(_featureFlagServiceMock.Object, _requestSchoolAccessorMock.Object);

        var result = await sut.WhatIsASimilarSchool("123456");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(school);
    }

    [Theory]
    [InlineData("Ks2", "123456")]
    [InlineData("Attendance", "123456")]
    [InlineData("ViewSimilarSchools", "123456")]
    [InlineData("SchoolDetails", "123456")]
    public async Task PrimaryPages_WhenFeatureEnabled_ReturnViewWithSchoolDetails(string actionName, string urn)
    {
        var school = CreateTestSchoolDetails(urn, "Test Primary School");
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync("EnablePrimarySchools"))
            .ReturnsAsync(true);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext>(), urn))
            .ReturnsAsync(school);

        var sut = new SchoolController(_featureFlagServiceMock.Object, _requestSchoolAccessorMock.Object);

        var result = actionName switch
        {
            "Ks2" => await sut.Ks2(urn),
            "Attendance" => await sut.Attendance(urn),
            "ViewSimilarSchools" => await sut.ViewSimilarSchools(urn),
            "SchoolDetails" => await sut.SchoolDetails(urn),
            _ => throw new InvalidOperationException($"Unexpected action '{actionName}'")
        };

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(school);
    }

    private static SchoolDetails CreateTestSchoolDetails(string urn, string name) =>
        new()
        {
            Name = name,
            Urn = urn,
            DfENumber = DataWithAvailability.Available("123/4567"),
            Ukprn = DataWithAvailability.Available("10012345"),
            Address = DataWithAvailability.Available("1 Test Street"),
            LocalAuthorityName = DataWithAvailability.Available("Test LA"),
            LocalAuthorityCode = DataWithAvailability.Available("123"),
            Region = DataWithAvailability.Available("Test Region"),
            UrbanRuralDescription = DataWithAvailability.Available("Urban"),
            AgeRangeLow = DataWithAvailability.Available(4),
            AgeRangeHigh = DataWithAvailability.Available(11),
            GenderOfEntry = DataWithAvailability.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability.Available("Primary"),
            SchoolType = DataWithAvailability.Available("Community school"),
            AdmissionsPolicy = DataWithAvailability.Available("Not applicable"),
            ReligiousCharacter = DataWithAvailability.Available("None"),
            GovernanceStructure = DataWithAvailability.NotAvailable<GovernanceType>(),
            AcademyTrustName = DataWithAvailability.NotAvailable<string>(),
            AcademyTrustId = DataWithAvailability.NotAvailable<string>(),
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(false),
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),
            HeadteacherName = DataWithAvailability.Available("Jane Smith"),
            Website = DataWithAvailability.NotAvailable<string>(),
            Telephone = DataWithAvailability.NotAvailable<string>(),
            Email = DataWithAvailability.NotAvailable<string>()
        };
}
