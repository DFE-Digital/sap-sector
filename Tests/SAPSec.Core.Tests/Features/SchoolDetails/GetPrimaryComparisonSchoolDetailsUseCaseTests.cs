using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolDetails.Comparison;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class GetPrimaryComparisonSchoolDetailsUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemorySimilarSchoolsPrimaryRepository _similarSchoolsRepo = new();
    private readonly Mock<ILogger<SchoolDetailsService>> _loggerMock = new();
    private readonly GetPrimaryComparisonSchoolDetailsUseCase _sut;

    public GetPrimaryComparisonSchoolDetailsUseCaseTests()
    {
        _sut = new GetPrimaryComparisonSchoolDetailsUseCase(
            _establishmentRepo,
            _similarSchoolsRepo,
            new SchoolDetailsService(_establishmentRepo, _loggerMock.Object));
    }

    [Fact]
    public async Task WhenCurrentSchoolUrnDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100002", "Comparator School"));

        var act = async () => await _sut.Execute(new("100001", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100001*");
    }

    [Fact]
    public async Task WhenComparatorSchoolUrnDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"));

        var act = async () => await _sut.Execute(new("100001", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100002*");
    }

    [Fact]
    public async Task WhenComparatorSchoolIsNotInSimilarSchoolsGroupForCurrentSchool_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"),
            Build.Establishment("100002", "Comparator School"));

        var act = async () => await _sut.Execute(new("100001", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100002*");
    }

    [Fact]
    public async Task SchoolName_SetToCurrentSchoolName()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"),
            Build.Establishment("100002", "Comparator School"));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.CurrentSchool.School.Name.Should().Be("Current School");
    }

    [Fact]
    public async Task CurrentSchoolCoordinates_CoordinatesConvertedToLatitudeAndLongitude()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .WithEastingNorthing(430000, 380000)),
            Build.Establishment("100002", "Comparator School"));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.CurrentSchool.Coordinates.Should().NotBeNull();
        response.CurrentSchool.Coordinates.Latitude.Should().BeApproximately(53.3160875, 0.0000001);
        response.CurrentSchool.Coordinates.Longitude.Should().BeApproximately(-1.5511531, 0.0000001);
    }

    [Fact]
    public async Task CurrentSchoolCoordinates_WhenCoordinatesMissingInData_CoordinatesNotAvailableInResponse()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"),
            Build.Establishment("100002", "Comparator School"));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.CurrentSchool.Coordinates.Should().BeNull();
    }

    [Fact]
    public async Task ComparatorSchoolCoordinates_CoordinatesConvertedToLatitudeAndLongitude()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"),
            Build.Establishment("100002", "Comparator School", x => x
                .WithEastingNorthing(431000, 381000)));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.ComparatorSchool.Coordinates.Should().NotBeNull();
        response.ComparatorSchool.Coordinates.Latitude.Should().BeApproximately(53.3250185, 0.0000001);
        response.ComparatorSchool.Coordinates.Longitude.Should().BeApproximately(-1.5360459, 0.0000001);
    }

    [Fact]
    public async Task ComparatorSchoolCoordinates_WhenCoordinatesMissingInData_CoordinatesNotAvailableInResponse()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"),
            Build.Establishment("100002", "Comparator School"));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.ComparatorSchool.Coordinates.Should().BeNull();
    }

    [Fact]
    public async Task DistanceMiles_CalculatedAsStraightLineDistanceUsingEastingAndNorthing()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .WithEastingNorthing(430000, 380000)),
            Build.Establishment("100002", "Comparator School", x => x
                .WithEastingNorthing(431000, 381000)));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.DistanceMiles.Should().BeApproximately(0.8787516, 0.0000001);
    }

    [Fact]
    public async Task DistanceMiles_WhenCurrentSchoolCoordinatesMissingInData_DistanceNotAvailableInResponse()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"),
            Build.Establishment("100002", "Comparator School", x => x
                .WithEastingNorthing(431000, 381000)));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.DistanceMiles.Should().BeNull();
    }

    [Fact]
    public async Task DistanceMiles_WhenComparatorSchoolCoordinatesMissingInData_DistanceNotAvailableInResponse()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .WithEastingNorthing(430000, 380000)),
            Build.Establishment("100002", "Comparator School"));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.DistanceMiles.Should().BeNull();
    }

    [Fact]
    public async Task ComparatorSchoolDetails_ContainsSchoolDetailsForComparatorSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"),
            Build.Establishment("100002", "Comparator School", x => x
                .WithTelephone("01234 567890")
                .WithWebsite("https://similar-school.example.com")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(new("100001", "100002"));

        response.ComparatorSchoolDetails.Urn.Should().Be("100002");
        response.ComparatorSchoolDetails.Name.Should().Be("Comparator School");
        response.ComparatorSchoolDetails.Telephone.Value.Should().Be("01234 567890");
        response.ComparatorSchoolDetails.Website.Value.Should().Be("https://similar-school.example.com");
    }
}
