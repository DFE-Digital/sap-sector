using SAPSec.Core.Features.Measures;
using SAPSec.Test.Common.Builders;
using static SAPSec.Core.Constants.Measures;

namespace SAPSec.Core.Tests.Features.Measures.Attendance;

public partial class GetComparisonAttendanceMeasuresUseCaseTests
{
    [Fact]
    public async Task Secondary_Absence_ContainsYearByYearValuesForCurrentAndSimilarSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")),
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "6.10", previous: "6.20", previous2: "6.30")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsenceSecondary(current: "6.10", previous: "6.90", previous2: "5.45")));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002"));
        var series = response.Absence.Series;

        series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 8.00m, 8.05m, 7.91m),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, 6.10m, 6.20m, 6.30m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 6.10m, 6.90m, 5.45m)
        ]);
    }

    [Fact]
    public async Task Secondary_Absence_WhenNoAbsenceDataForSimilarSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002"));
        var series = response.Absence.Series
            .First(s => s.SeriesType == MeasureSeriesType.SimilarSchool);

        series.Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Secondary_Absence_WhenNoAbsenceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002"));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Secondary_Absence_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "", previous: "", previous2: "")),
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "", previous: "", previous2: "")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsenceSecondary(current: "", previous: "", previous2: "")));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002"));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Secondary_Absence_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "x", previous: "y2", previous2: "3z")),
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "x", previous: "y2", previous2: "3z")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsenceSecondary(current: "x", previous: "y2", previous2: "3z")));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002"));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [Fact]
    public async Task Secondary_Absence_FilterBy_IgnoresInvalidFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "18", previous: "75", previous2: "80")));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002", new()
        {
            ["xxx"] = "1",
            [Absence.Filters.Type.Key] = Absence.Filters.Type.Values.Overall,
            ["yyy"] = "2",
        }));

        response.Absence.Series.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Secondary_Absence_FilterBy_Type_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "81", previous: "80", previous2: "79")
                .WithPersistentAbsence(current: "", previous: "", previous2: "")),
            Build.Absence.Establishment("100002", x => x
                .WithOverallAbsence(current: "81", previous: "80", previous2: "79")
                .WithPersistentAbsence(current: "", previous: "", previous2: "")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsenceSecondary(current: "81", previous: "80", previous2: "79")
                .WithPersistentAbsenceSecondary(current: "", previous: "", previous2: "")));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002", new()
        {
            [Absence.Filters.Type.Key] = Absence.Filters.Type.Values.Persistent
        }));

        var series = response.Absence.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Absence.Filters.Type.Values.Overall, 8.00, 6.10, 6.10)]
    [InlineData(Absence.Filters.Type.Values.Persistent, 2.27, 1.24, 3.20)]
    //Empty or invalid filter values default to Overall absence
    [InlineData("", 8.00, 6.10, 6.10)]
    [InlineData("xyz", 8.00, 6.10, 6.10)]
    [Theory]
    public async Task Secondary_Absence_FilterBy_Type_ContainsCurrentYearValuesForSelectedType(
        string type, double currentSchool, double similarSchool, double england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")
                .WithPersistentAbsence(current: "2.27", previous: "1.24", previous2: "8.20")),
            Build.Absence.Establishment("100002", x => x
                .WithOverallAbsence(current: "6.10", previous: "6.20", previous2: "6.30")
                .WithPersistentAbsence(current: "1.24", previous: "1.30", previous2: "1.40")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsenceSecondary(current: "6.10", previous: "6.90", previous2: "5.45")
                .WithPersistentAbsenceSecondary(current: "3.20", previous: "2.24", previous2: "2.20")));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002", new()
        {
            [Absence.Filters.Type.Key] = type
        }));

        var series = response.Absence.Series;

        series.First(s => s.SeriesType == MeasureSeriesType.CurrentSchool).Current.Should().Be((decimal?)currentSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.SimilarSchool).Current.Should().Be((decimal?)similarSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage).Current.Should().Be((decimal?)england);
    }

    [Fact]
    public async Task Secondary_Absence_FilterBy_PupilCharacteristic_WhenMissing_DefaultsToAllPupils()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")
                .WithOverallAbsenceBoys(current: "7.00", previous: "7.05", previous2: "6.91")));

        var withoutFilter = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002"));
        var withExplicitAllPupils = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002", new()
        {
            [Absence.Filters.PupilCharacteristic.Key] = Absence.Filters.PupilCharacteristic.Values.AllPupils
        }));

        withoutFilter.Absence.Series.Should().Equal(withExplicitAllPupils.Absence.Series);
    }

    [InlineData(Absence.Filters.PupilCharacteristic.Values.Boys, 7.00, 6.10, 2.24)]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.Girls, 6.00, 5.10, 1.24)]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.Fsm, 9.00, 8.10, 5.24)]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.NonFsm, 4.00, 3.10, 0.24)]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.Eal, 3.00, 2.10, 9.24)]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.Efl, 2.00, 1.10, 8.24)]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.AllPupils, 8.00, 6.10, 6.10)]
    [Theory]
    public async Task Secondary_Absence_FilterBy_PupilCharacteristic_ContainsCurrentYearValuesForSelectedCharacteristic(
        string characteristic, double currentSchool, double similarSchool, double england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")
                .WithOverallAbsenceBoys(current: "7.00", previous: "7.05", previous2: "6.91")
                .WithOverallAbsenceGirls(current: "6.00", previous: "6.05", previous2: "5.91")
                .WithOverallAbsenceFsm(current: "9.00", previous: "9.05", previous2: "8.91")
                .WithOverallAbsenceNonFsm(current: "4.00", previous: "4.05", previous2: "3.91")
                .WithOverallAbsenceEal(current: "3.00", previous: "3.05", previous2: "2.91")
                .WithOverallAbsenceEfl(current: "2.00", previous: "2.05", previous2: "1.91")),
            Build.Absence.Establishment("100002", x => x
                .WithOverallAbsence(current: "6.10", previous: "6.20", previous2: "6.30")
                .WithOverallAbsenceBoys(current: "6.10", previous: "6.20", previous2: "6.30")
                .WithOverallAbsenceGirls(current: "5.10", previous: "5.20", previous2: "5.30")
                .WithOverallAbsenceFsm(current: "8.10", previous: "8.20", previous2: "8.30")
                .WithOverallAbsenceNonFsm(current: "3.10", previous: "3.20", previous2: "3.30")
                .WithOverallAbsenceEal(current: "2.10", previous: "2.20", previous2: "2.30")
                .WithOverallAbsenceEfl(current: "1.10", previous: "1.20", previous2: "1.30")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsenceSecondary(current: "6.10", previous: "6.90", previous2: "5.45")
                .WithOverallAbsenceBoysSecondary(current: "2.24", previous: "1.20", previous2: "0.20")
                .WithOverallAbsenceGirlsSecondary(current: "1.24", previous: "0.20", previous2: "9.20")
                .WithOverallAbsenceFsmSecondary(current: "5.24", previous: "4.20", previous2: "3.20")
                .WithOverallAbsenceNonFsmSecondary(current: "0.24", previous: "9.20", previous2: "8.20")
                .WithOverallAbsenceEalSecondary(current: "9.24", previous: "8.20", previous2: "7.20")
                .WithOverallAbsenceEflSecondary(current: "8.24", previous: "7.20", previous2: "6.20")));

        var response = await _sut.Execute(Request(MeasurePhase.Secondary, "100001", "100002", new()
        {
            [Absence.Filters.PupilCharacteristic.Key] = characteristic
        }));

        var series = response.Absence.Series;

        series.First(s => s.SeriesType == MeasureSeriesType.CurrentSchool).Current.Should().Be((decimal?)currentSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.SimilarSchool).Current.Should().Be((decimal?)similarSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage).Current.Should().Be((decimal?)england);
    }
}
