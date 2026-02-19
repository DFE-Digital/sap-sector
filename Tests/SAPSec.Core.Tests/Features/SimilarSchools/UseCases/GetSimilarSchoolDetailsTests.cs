using Moq;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class GetSimilarSchoolDetailsTests
{
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepo;
    private readonly Mock<ISchoolDetailsService> _schoolDetailsService;
    private readonly GetSimilarSchoolDetails _sut;

    public GetSimilarSchoolDetailsTests()
    {
        _similarSchoolsRepo = new Mock<ISimilarSchoolsSecondaryRepository>();
        _schoolDetailsService = new Mock<ISchoolDetailsService>();

        _sut = new GetSimilarSchoolDetails(
            _similarSchoolsRepo.Object,
            _schoolDetailsService.Object);
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
        SetupSimilarSchools(CurrentSchool("100001", b => b.WithName("Test School")), [
            SimilarSchool("100002"),
            SimilarSchool("100003"),
            SimilarSchool("100004")
        ]);

        SetupSchoolDetails(SchoolDetails("100001", b => b.WithName("Test School")));

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

    private void SetupSimilarSchools(SimilarSchool currentSchool, List<SimilarSchool> similarSchools)
    {
        _similarSchoolsRepo.Setup(r => r.GetSimilarSchoolsGroupAsync(currentSchool.URN))
            .ReturnsAsync((currentSchool, similarSchools.AsReadOnly()));
    }

    private void SetupSchoolDetails(SchoolDetails schoolDetails)
    {
        _schoolDetailsService.Setup(s => s.GetByUrnAsync(schoolDetails.Urn))
            .ReturnsAsync(schoolDetails);
    }

    private SimilarSchool CurrentSchool(string urn, Func<SimilarSchoolBuilder, SimilarSchoolBuilder> build = null)
    {
        build ??= b => b;
        var builder = new SimilarSchoolBuilder(urn);
        return build(builder).Build();
    }

    private SimilarSchool SimilarSchool(string urn, Func<SimilarSchoolBuilder, SimilarSchoolBuilder> build = null)
    {
        build ??= b => b;
        var builder = new SimilarSchoolBuilder(urn);
        return build(builder).Build();
    }

    private SchoolDetails SchoolDetails(string urn, Func<SchoolDetailsBuilder, SchoolDetailsBuilder> build = null)
    {
        build ??= b => b;
        var builder = new SchoolDetailsBuilder(urn);
        return build(builder).Build();
    }
}

