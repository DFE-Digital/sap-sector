using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.Measures.Attendance;

public partial class GetComparisonAttendanceMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryAbsenceRepository _absenceRepo;
    private readonly GetComparisonAttendanceMeasuresUseCase _sut;

    public GetComparisonAttendanceMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _absenceRepo = new(_establishmentRepo);
        _sut = new GetComparisonAttendanceMeasuresUseCase(
            _establishmentRepo,
            _absenceRepo);
    }

    [Theory]
    [InlineData(MeasurePhase.Primary)]
    [InlineData(MeasurePhase.Secondary)]
    public async Task WhenCurrentSchoolDoesNotExist_ThrowsNotFoundException(MeasurePhase phase)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100002", "Test School 2"));

        var act = async () => await _sut.Execute(Request(phase, "999999", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Theory]
    [InlineData(MeasurePhase.Primary)]
    [InlineData(MeasurePhase.Secondary)]
    public async Task WhenSimilarSchoolDoesNotExist_ThrowsNotFoundException(MeasurePhase phase)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1"));

        var act = async () => await _sut.Execute(Request(phase, "100001", "999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Theory]
    [InlineData(MeasurePhase.Primary)]
    [InlineData(MeasurePhase.Secondary)]
    public async Task Absence_ShouldContainExpectedMeasureSeries(MeasurePhase phase)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1"),
            Build.Establishment("100002", "Test School 2"));

        var response = await _sut.Execute(Request(phase, "100001", "100002"));

        var seriesTypes = response.Absence.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    private static GetComparisonAttendanceMeasuresRequest Request(
        MeasurePhase phase,
        string urn,
        string similarSchoolUrn,
        Dictionary<string, string>? filterBy = null) =>
            new(phase, urn, similarSchoolUrn, filterBy);

}
