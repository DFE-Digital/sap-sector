using Moq;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class GetPrimarySimilarSchoolDetailsTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly Mock<ISchoolDetailsService> _schoolDetailsService = new();
    private readonly GetPrimarySimilarSchoolDetailsUseCase _sut;

    public GetPrimarySimilarSchoolDetailsTests()
    {
        _sut = new GetPrimarySimilarSchoolDetailsUseCase(_establishmentRepo, _schoolDetailsService.Object);

        _schoolDetailsService
            .Setup(s => s.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync((string urn) => SchoolDetails(urn));
    }

    [Fact]
    public async Task WhenCurrentSchoolUrnDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100002", EstablishmentName = "Similar School" });

        var act = async () => await _sut.Execute(new("100001", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100001*");
    }

    [Fact]
    public async Task WhenSimilarSchoolUrnDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School" });

        var act = async () => await _sut.Execute(new("100001", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100002*");
    }

    [Fact]
    public async Task SchoolName_SetToCurrentSchoolName()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School" },
            new Establishment { URN = "100002", EstablishmentName = "Similar School" });

        var response = await _sut.Execute(new("100001", "100002"));

        response.SchoolName.Should().Be("Current School");
    }

    [Fact]
    public async Task CurrentSchoolCoordinates_CoordinatesConvertedToLatitudeAndLongitude()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School", Easting = 430000, Northing = 380000 },
            new Establishment { URN = "100002", EstablishmentName = "Similar School" });

        var response = await _sut.Execute(new("100001", "100002"));

        response.CurrentSchoolCoordinates.Should().NotBeNull();
    }

    [Fact]
    public async Task CurrentSchoolCoordinates_WhenCoordinatesMissingInData_CoordinatesNotAvailableInResponse()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School" },
            new Establishment { URN = "100002", EstablishmentName = "Similar School" });

        var response = await _sut.Execute(new("100001", "100002"));

        response.CurrentSchoolCoordinates.Should().BeNull();
    }

    [Fact]
    public async Task SimilarSchoolCoordinates_CoordinatesConvertedToLatitudeAndLongitude()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School" },
            new Establishment { URN = "100002", EstablishmentName = "Similar School", Easting = 431000, Northing = 381000 });

        var response = await _sut.Execute(new("100001", "100002"));

        response.SimilarSchoolCoordinates.Should().NotBeNull();
    }

    [Fact]
    public async Task SimilarSchoolCoordinates_WhenCoordinatesMissingInData_CoordinatesNotAvailableInResponse()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School" },
            new Establishment { URN = "100002", EstablishmentName = "Similar School" });

        var response = await _sut.Execute(new("100001", "100002"));

        response.SimilarSchoolCoordinates.Should().BeNull();
    }

    [Fact]
    public async Task DistanceMiles_CalculatedAsStraightLineDistanceUsingEastingAndNorthing()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School", Easting = 430000, Northing = 380000 },
            new Establishment { URN = "100002", EstablishmentName = "Similar School", Easting = 431000, Northing = 381000 });

        var response = await _sut.Execute(new("100001", "100002"));

        response.DistanceMiles.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DistanceMiles_WhenCurrentSchoolCoordinatesMissingInData_DistanceNotAvailableInResponse()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School" },
            new Establishment { URN = "100002", EstablishmentName = "Similar School", Easting = 431000, Northing = 381000 });

        var response = await _sut.Execute(new("100001", "100002"));

        response.DistanceMiles.Should().BeNull();
    }

    [Fact]
    public async Task DistanceMiles_WhenSimilarSchoolCoordinatesMissingInData_DistanceNotAvailableInResponse()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School", Easting = 430000, Northing = 380000 },
            new Establishment { URN = "100002", EstablishmentName = "Similar School" });

        var response = await _sut.Execute(new("100001", "100002"));

        response.DistanceMiles.Should().BeNull();
    }

    [Fact]
    public async Task SimilarSchoolDetails_ContainsSchoolDetailsForSimilarSchool()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School" },
            new Establishment { URN = "100002", EstablishmentName = "Similar School" });

        var response = await _sut.Execute(new("100001", "100002"));

        response.SimilarSchoolDetails.Urn.Should().Be("100002");
        _schoolDetailsService.Verify(s => s.GetByUrnAsync("100002"), Times.Once);
    }

    private static SchoolDetails SchoolDetails(string urn, Func<SchoolDetailsBuilder, SchoolDetailsBuilder>? build = null)
    {
        build ??= b => b;
        var builder = new SchoolDetailsBuilder(urn);
        return build(builder).Build();
    }
}
