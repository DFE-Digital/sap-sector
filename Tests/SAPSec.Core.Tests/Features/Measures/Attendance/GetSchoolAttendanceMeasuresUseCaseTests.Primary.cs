using SAPSec.Core.Features.Measures;
using SAPSec.Test.Common.Builders;
using static SAPSec.Core.Constants.Measures;

namespace SAPSec.Core.Tests.Features.Measures.Attendance;

public partial class GetSchoolAttendanceMeasuresUseCaseTests
{
    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Primary_Absence_WhenNoAbsenceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001"));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Primary_Absence_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "", previous: "", previous2: "")));

        _absenceRepo.SetupLAAbsence(
             Build.Absence.LA("001", x => x.WithOverallAbsencePrimary(current: "", previous: "", previous2: "")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "", previous: "", previous2: "")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001"));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Primary_Absence_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "x", previous: "y2", previous2: "3z")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsencePrimary(current: "x", previous: "y2", previous2: "3z")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "x", previous: "y2", previous2: "3z")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001"));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 8.00, 8.05, 7.91)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 7.05, 7.10, 6.20)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 6.10, 6.90, 5.45)]
    [Theory]
    public async Task Primary_Absence_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsencePrimary(current: "7.05", previous: "7.10", previous2: "6.20")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "6.10", previous: "6.90", previous2: "5.45")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001"));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task Primary_Absence_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("XYZ")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsencePrimary(current: "71", previous: "70", previous2: "69")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, urn));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Primary_Absence_FilterBy_IgnoresInvalidFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "18", previous: "75", previous2: "80")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001", filterBy: new()
        {
            ["xxx"] = "1",
            [Absence.Filters.Type.Key] = Absence.Filters.Type.Values.Overall,
            ["yyy"] = "2",
        }));

        response.Absence.Series.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Primary_Absence_FilterBy_Type_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "81", previous: "80", previous2: "79")
                .WithPersistentAbsence(current: "", previous: "", previous2: "")));

        _absenceRepo.SetupLAAbsence(
             Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "81", previous: "80", previous2: "79")
                .WithPersistentAbsencePrimary(current: "", previous: "", previous2: "")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "81", previous: "80", previous2: "79")
                .WithPersistentAbsencePrimary(current: "", previous: "", previous2: "")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001", filterBy: new()
        {
            [Absence.Filters.Type.Key] = Absence.Filters.Type.Values.Persistent
        }));

        var series = response.Absence.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Absence.Filters.Type.Values.Overall, new[] { 5.20, 6.04, 4.30 }, new[] { 8.24, 5.44, 9.34 }, new[] { 3.24, 2.20, 1.20 })]
    [InlineData(Absence.Filters.Type.Values.Persistent, new[] { 2.27, 1.24, 8.20 }, new[] { 7.23, 7.29, 5.20 }, new[] { 3.20, 2.24, 2.20 })]
    //Empty or invalid filter values default to Overall absence
    [InlineData("", new[] { 5.20, 6.04, 4.30 }, new[] { 8.24, 5.44, 9.34 }, new[] { 3.24, 2.20, 1.20 })]
    [InlineData("xyz", new[] { 5.20, 6.04, 4.30 }, new[] { 8.24, 5.44, 9.34 }, new[] { 3.24, 2.20, 1.20 })]
    [Theory]
    public async Task Primary_Absence_FilterBy_Type_ContainsYearByYearValuesForSelectedType(string type, double[] currentSchool, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "5.20", previous: "6.04", previous2: "4.30")
                .WithPersistentAbsence(current: "2.27", previous: "1.24", previous2: "8.20")));

        _absenceRepo.SetupLAAbsence(
             Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "8.24", previous: "5.44", previous2: "9.34")
                .WithPersistentAbsencePrimary(current: "7.23", previous: "7.29", previous2: "5.20")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "3.24", previous: "2.20", previous2: "1.20")
                .WithPersistentAbsencePrimary(current: "3.20", previous: "2.24", previous2: "2.20")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001", filterBy: new()
        {
            [Absence.Filters.Type.Key] = type
        }));

        var series = response.Absence.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task Primary_Absence_FilterBy_PupilCharacteristic_WhenMissing_DefaultsToAllPupils()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")
                .WithOverallAbsenceBoys(current: "7.00", previous: "7.05", previous2: "6.91")));

        var withoutFilter = await _sut.Execute(Request(MeasurePhase.Primary, "100001"));
        var withExplicitAllPupils = await _sut.Execute(Request(MeasurePhase.Primary, "100001", filterBy: new()
        {
            [Absence.Filters.PupilCharacteristic.Key] = Absence.Filters.PupilCharacteristic.Values.AllPupils
        }));

        withoutFilter.Absence.Series.Should().Equal(withExplicitAllPupils.Absence.Series);
    }

    [InlineData(Absence.Filters.PupilCharacteristic.Values.Boys, new[] { 7.00, 7.05, 6.91 }, new[] { 6.24, 6.44, 6.34 }, new[] { 2.24, 1.20, 0.20 })]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.Girls, new[] { 6.00, 6.05, 5.91 }, new[] { 5.24, 5.44, 5.34 }, new[] { 1.24, 0.20, 9.20 })]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.Fsm, new[] { 9.00, 9.05, 8.91 }, new[] { 9.24, 9.44, 9.34 }, new[] { 5.24, 4.20, 3.20 })]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.NonFsm, new[] { 4.00, 4.05, 3.91 }, new[] { 4.24, 4.44, 4.34 }, new[] { 0.24, 9.20, 8.20 })]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.Eal, new[] { 3.00, 3.05, 2.91 }, new[] { 3.24, 3.44, 3.34 }, new[] { 9.24, 8.20, 7.20 })]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.Efl, new[] { 2.00, 2.05, 1.91 }, new[] { 2.24, 2.44, 2.34 }, new[] { 8.24, 7.20, 6.20 })]
    [InlineData(Absence.Filters.PupilCharacteristic.Values.AllPupils, new[] { 8.00, 8.05, 7.91 }, new[] { 7.05, 7.10, 6.20 }, new[] { 6.10, 6.90, 5.45 })]
    [Theory]
    public async Task Primary_Absence_FilterBy_PupilCharacteristic_ContainsYearByYearValuesForSelectedCharacteristic(string characteristic, double[] currentSchool, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                .WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")
                .WithOverallAbsenceBoys(current: "7.00", previous: "7.05", previous2: "6.91")
                .WithOverallAbsenceGirls(current: "6.00", previous: "6.05", previous2: "5.91")
                .WithOverallAbsenceFsm(current: "9.00", previous: "9.05", previous2: "8.91")
                .WithOverallAbsenceNonFsm(current: "4.00", previous: "4.05", previous2: "3.91")
                .WithOverallAbsenceEal(current: "3.00", previous: "3.05", previous2: "2.91")
                .WithOverallAbsenceEfl(current: "2.00", previous: "2.05", previous2: "1.91")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "7.05", previous: "7.10", previous2: "6.20")
                .WithOverallAbsenceBoysPrimary(current: "6.24", previous: "6.44", previous2: "6.34")
                .WithOverallAbsenceGirlsPrimary(current: "5.24", previous: "5.44", previous2: "5.34")
                .WithOverallAbsenceFsmPrimary(current: "9.24", previous: "9.44", previous2: "9.34")
                .WithOverallAbsenceNonFsmPrimary(current: "4.24", previous: "4.44", previous2: "4.34")
                .WithOverallAbsenceEalPrimary(current: "3.24", previous: "3.44", previous2: "3.34")
                .WithOverallAbsenceEflPrimary(current: "2.24", previous: "2.44", previous2: "2.34")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "6.10", previous: "6.90", previous2: "5.45")
                .WithOverallAbsenceBoysPrimary(current: "2.24", previous: "1.20", previous2: "0.20")
                .WithOverallAbsenceGirlsPrimary(current: "1.24", previous: "0.20", previous2: "9.20")
                .WithOverallAbsenceFsmPrimary(current: "5.24", previous: "4.20", previous2: "3.20")
                .WithOverallAbsenceNonFsmPrimary(current: "0.24", previous: "9.20", previous2: "8.20")
                .WithOverallAbsenceEalPrimary(current: "9.24", previous: "8.20", previous2: "7.20")
                .WithOverallAbsenceEflPrimary(current: "8.24", previous: "7.20", previous2: "6.20")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001", filterBy: new()
        {
            [Absence.Filters.PupilCharacteristic.Key] = characteristic
        }));

        var series = response.Absence.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task Primary_Absence_FilterBy_TypeAndPupilCharacteristic_Combined_ContainsExpectedValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x
                // Overall x Boys - should NOT be picked when Type=Persistent
                .WithOverallAbsenceBoys(current: "1", previous: "1", previous2: "1")
                // Persistent x All pupils - should NOT be picked when Characteristic=Boys
                .WithPersistentAbsence(current: "2", previous: "2", previous2: "2")
                // Persistent x Boys - the combination actually selected
                .WithPersistentAbsenceBoys(current: "45", previous: "44", previous2: "43")));

        var response = await _sut.Execute(Request(MeasurePhase.Primary, "100001", filterBy: new()
        {
            [Absence.Filters.Type.Key] = Absence.Filters.Type.Values.Persistent,
            [Absence.Filters.PupilCharacteristic.Key] = Absence.Filters.PupilCharacteristic.Values.Boys
        }));

        var currentSchoolSeries = response.Absence.Series.First(s => s.SeriesType == MeasureSeriesType.CurrentSchool);

        currentSchoolSeries.Should().Be(new MeasureSeries(MeasureSeriesType.CurrentSchool, 45, 44, 43));
    }
}