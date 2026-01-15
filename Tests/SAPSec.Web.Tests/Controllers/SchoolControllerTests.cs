using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Search;
using SAPSec.Core.Services;
using SAPSec.Infrastructure.Entities;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolControllerTests
{
    private readonly Mock<ILogger<SchoolController>> _mockLogger;
    private readonly Mock<IEstablishmentService> _mockEstablishmentService;
    private readonly SchoolController _controller;

    private static Establishment FakeEstablishment1 = new()
    {
        URN = "123456",
        UKPRN = "10",
        LAId = "100",
        EstablishmentNumber = "1",
        EstablishmentName = "Fake Establishment One",
        LANAme = "Leeds",
        Easting = "430000",
        Northing = "433000",
        Latitude = "53.8",
        Longitude = "-1.55"
    };

    public SchoolControllerTests()
    {
        _mockLogger = new Mock<ILogger<SchoolController>>();
        _mockEstablishmentService = new Mock<IEstablishmentService>();
        _mockEstablishmentService.Setup(s => s.GetEstablishment(FakeEstablishment1.URN))
            .Returns(FakeEstablishment1);

        _controller = new SchoolController(_mockEstablishmentService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Index_Get_ReturnsViewResult()
    {
        var result = _controller.Index(FakeEstablishment1.URN);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Ks4HeadlineMeasures_Get_ReturnsViewResult()
    {
        var result = _controller.Ks4HeadlineMeasures(FakeEstablishment1.URN);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Ks4CoreSubjects_Get_ReturnsViewResult()
    {
        var result = _controller.Ks4CoreSubjects(FakeEstablishment1.URN);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Attendance_Get_ReturnsViewResult()
    {
        var result = _controller.Attendance(FakeEstablishment1.URN);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void ViewSimilarSchools_Get_ReturnsViewResult()
    {
        var result = _controller.ViewSimilarSchools(FakeEstablishment1.URN);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void SchoolDetails_Get_ReturnsViewResult()
    {
        var result = _controller.SchoolDetails(FakeEstablishment1.URN);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void WhatIsASimilarSchool_Get_ReturnsViewResult()
    {
        var result = _controller.WhatIsASimilarSchool(FakeEstablishment1.URN);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }
}