using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.Controllers;

namespace SAPSec.Web.Tests.Deprecated.Controllers;

public class PrimarySchoolControllerTests
{
    private readonly Mock<IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse>> _schoolInfoUseCaseMock = new();
    private readonly Mock<IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse>> _getKs2PerformanceMeasuresUseCaseMock = new();
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock = new();

    [Fact]
    public async Task Index_WhenFeatureDisabled_ReturnsNotFound()
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync("EnablePrimarySchools"))
            .ReturnsAsync(false);

        var sut = new SchoolController(_schoolInfoUseCaseMock.Object, _getKs2PerformanceMeasuresUseCaseMock.Object, _featureFlagServiceMock.Object);

        var result = await sut.Index("123456");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenFeatureEnabled_ReturnsViewWithSchoolDetails()
    {
        var school = new SchoolInfo("123456", "Test Primary School", new Address("1 Test Street"));
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync("EnablePrimarySchools"))
            .ReturnsAsync(true);
        _schoolInfoUseCaseMock
            .Setup(x => x.Execute(new("123456")))
            .ReturnsAsync(new GetSchoolInfoResponse(school));

        var sut = new SchoolController(_schoolInfoUseCaseMock.Object, _getKs2PerformanceMeasuresUseCaseMock.Object, _featureFlagServiceMock.Object);

        var result = await sut.Index("123456");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(school);
        sut.ViewData["SchoolDetails"].Should().Be(school);
        sut.ViewData["BreadcrumbNode"].Should().NotBeNull();
    }

    [Fact]
    public async Task WhatIsASimilarSchool_WhenFeatureEnabled_ReturnsViewWithSchoolDetails()
    {
        var school = new SchoolInfo("123456", "Test Primary School", new Address("1 Test Street"));
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync("EnablePrimarySchools"))
            .ReturnsAsync(true);
        _schoolInfoUseCaseMock
            .Setup(x => x.Execute(new("123456")))
            .ReturnsAsync(new GetSchoolInfoResponse(school));

        var sut = new SchoolController(_schoolInfoUseCaseMock.Object, _getKs2PerformanceMeasuresUseCaseMock.Object, _featureFlagServiceMock.Object);

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
        var school = new SchoolInfo(urn, "Test Primary School", new Address("1 Test Street"));
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync("EnablePrimarySchools"))
            .ReturnsAsync(true);
        _schoolInfoUseCaseMock
            .Setup(x => x.Execute(new(urn)))
            .ReturnsAsync(new GetSchoolInfoResponse(school));

        var sut = new SchoolController(_schoolInfoUseCaseMock.Object, _getKs2PerformanceMeasuresUseCaseMock.Object, _featureFlagServiceMock.Object);

        var result = actionName switch
        {
            "Ks2" => await sut.Ks2PerformanceMeasures(urn),
            "Attendance" => await sut.Attendance(urn),
            "ViewSimilarSchools" => await sut.ViewSimilarSchools(urn),
            "SchoolDetails" => await sut.SchoolDetails(urn),
            _ => throw new InvalidOperationException($"Unexpected action '{actionName}'")
        };

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(school);
    }
}
