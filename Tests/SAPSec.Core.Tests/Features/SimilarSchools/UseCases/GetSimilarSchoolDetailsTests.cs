using Moq;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class GetSimilarSchoolDetailsTests
{
    private readonly InMemorySimilarSchoolsSecondaryRepository _similarSchoolsRepo = new();
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemoryKs4PerformanceRepository _performanceRepo = new();
    private readonly InMemoryAbsenceRepository _absenceRepo = new();
    private readonly Mock<ISchoolDetailsService> _schoolDetailsService = new Mock<ISchoolDetailsService>();
    private readonly GetSimilarSchoolDetails _sut;

    public GetSimilarSchoolDetailsTests()
    {
        _sut = new GetSimilarSchoolDetails(
            _establishmentRepo,
            _similarSchoolsRepo,
            _schoolDetailsService.Object,
            _performanceRepo,
            _absenceRepo);
    }

    [Fact(Skip = "TODO")]
    public async Task WhenCurrentSchoolUrnIsInvalid_ReturnsValidationError()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task WhenCurrentSchoolUrnDoesNotExist_ReturnsNotFoundError()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task WhenSimilarSchoolUrnDoesNotExist_ReturnsNotFoundError()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task WhenSimilarSchoolUrnDoesNotExistInSimilarSchoolsGroupForCurrentSchool_ReturnsNotFoundError()
    {
    }

    [Fact]
    public async Task SchoolName_SetToCurrentSchoolName()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", EstablishmentName = "Test School" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" }
        );

        _schoolDetailsService.Setup(s => s.GetByUrnAsync("100001"))
            .ReturnsAsync(SchoolDetails("100001", b => b.WithName("Test School")));

        var response = await _sut.Execute(new("100001", "100002"));

        response.Should().NotBeNull();

        response.SchoolName.Should().Be("Test School");
    }

    [Fact(Skip = "TODO")]
    public async Task CurrentSchoolCoordinates_WhenCoordinatesMissingInData_CoordinatesNotAvailableInResponse()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task CurrentSchoolCoordinates_CoordinatesConvertedToLatitudeAndLongitude()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task SimilarSchoolCoordinates_WhenCoordinatesMissingInData_CoordinatesNotAvailableInResponse()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task SimilarSchoolCoordinates_CoordinatesConvertedToLatitudeAndLongitude()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task DistanceMiles_WhenCurrentSchoolCoordinatesMissingInData_DistanceNotAvailableInResponse()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task DistanceMiles_WhenSimilarSchoolCoordinatesMissingInData_DistanceNotAvailableInResponse()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task DistanceMiles_CalculatedAsStraightLineDistanceUsingEastingAndNorthing()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task SimilarSchoolDetails_ContainsSchoolDetailsForSimilarSchool()
    {
    }

    private SchoolDetails SchoolDetails(string urn, Func<SchoolDetailsBuilder, SchoolDetailsBuilder> build = null)
    {
        build ??= b => b;
        var builder = new SchoolDetailsBuilder(urn);
        return build(builder).Build();
    }
}

