using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Primary;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Constants.Measures;

namespace SAPSec.Core.Tests.Features.Primary;

public class GetSchoolAttendanceComparisonUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryAbsenceRepository _absenceRepo;
    private readonly GetSchoolAttendanceComparisonUseCase _sut;

    public GetSchoolAttendanceComparisonUseCaseTests()
    {
        _establishmentRepo = new();
        _absenceRepo = new(_establishmentRepo);
        _sut = new GetSchoolAttendanceComparisonUseCase(
            _establishmentRepo,
            _absenceRepo);
    }

    private static GetSchoolAttendanceComparisonRequest Request(
        string urn,
        string similarSchoolUrn,
        Dictionary<string, string>? filterBy = null) =>
        new(urn, similarSchoolUrn, filterBy);

    [Fact]
    public async Task WhenCurrentSchoolDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100002", "Test School 2", x => x.Primary()));

        var act = async () => await _sut.Execute(Request("999999", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task WhenSimilarSchoolDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        var act = async () => await _sut.Execute(Request("100001", "999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task Absence_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var seriesTypes = response.Absence.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task Absence_ContainsYearByYearValuesForCurrentAndSimilarSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")),
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "6.10", previous: "6.20", previous2: "6.30")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "6.10", previous: "6.90", previous2: "5.45")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.Absence.Series;

        series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 8.00m, 8.05m, 7.91m),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, 6.10m, 6.20m, 6.30m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 6.10m, 6.90m, 5.45m)
        ]);
    }

    [Fact]
    public async Task Absence_WhenNoAbsenceDataForSimilarSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.Absence.Series
            .First(s => s.SeriesType == MeasureSeriesType.SimilarSchool);

        series.Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null));
    }

    [Theory]
    [InlineData(Absence.Filters.Type.Values.Overall, 8.00, 6.10, 6.10)]
    [InlineData(Absence.Filters.Type.Values.Persistent, 2.27, 1.24, 3.20)]
    public async Task Absence_FilterBy_Type_ContainsCurrentYearValuesForSelectedType(
        string type, double currentSchool, double similarSchool, double england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")
                .WithPersistentAbsence(current: "2.27", previous: "1.24", previous2: "8.20")),
            Build.Absence.Establishment("100002", x => x
                .WithOverallAbsence(current: "6.10", previous: "6.20", previous2: "6.30")
                .WithPersistentAbsence(current: "1.24", previous: "1.30", previous2: "1.40")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "6.10", previous: "6.90", previous2: "5.45")
                .WithPersistentAbsencePrimary(current: "3.20", previous: "2.24", previous2: "2.20")));

        var response = await _sut.Execute(Request("100001", "100002", new()
        {
            [Absence.Filters.Type.Key] = type
        }));

        var series = response.Absence.Series;

        series.First(s => s.SeriesType == MeasureSeriesType.CurrentSchool).Current.Should().Be((decimal?)currentSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.SimilarSchool).Current.Should().Be((decimal?)similarSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage).Current.Should().Be((decimal?)england);
    }
}
