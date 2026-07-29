using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Constants.Measures;
using static SAPSec.Core.Constants.Measures.Primary;

namespace SAPSec.Core.Tests.Features.Primary;

public class GetSchoolAttendanceMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryAbsenceRepository _absenceRepo;
    private readonly GetSchoolAttendanceMeasuresUseCase _sut;

    public GetSchoolAttendanceMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _absenceRepo = new(_establishmentRepo);
        _sut = new GetSchoolAttendanceMeasuresUseCase(
            _establishmentRepo,
            _absenceRepo);
    }

    [Fact]
    public async Task WhenCurrentSchoolDoesNotExist_ThrowsNotFoundException()
    {
        var act = async () => await _sut.Execute(Request("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task School_ShouldContainCurrentSchoolInfo()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x
                .Primary()
                .WithAddress("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG")
                .InLA("001", "Test LA")));

        var response = await _sut.Execute(Request("100001"));

        response.School.Urn.Should().Be("100001");
        response.School.Name.Should().Be("Test School");
        response.School.Address.Should().Be(
            new Address("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG"));
        response.School.LocalAuthority.Should().Be(
            new LocalAuthority("001", "Test LA"));
    }

    [Fact]
    public async Task Absence_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.Absence.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
                MeasureSeriesType.LASchoolsAverage,
                MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Absence_WhenNoAbsenceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

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
    public async Task Absence_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "", previous: "", previous2: "")));

       _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsencePrimary(current: "", previous: "", previous2: "")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "", previous: "", previous2: "")));

        var response = await _sut.Execute(Request("100001"));

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
    public async Task Absence_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100002", x => x.WithOverallAbsence(current: "x", previous: "y2", previous2: "3z")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsencePrimary(current: "x", previous: "y2", previous2: "3z")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "x", previous: "y2", previous2: "3z")));

        var response = await _sut.Execute(Request("100001"));

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
    public async Task Absence_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsencePrimary(current: "7.05", previous: "7.10", previous2: "6.20")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x.WithOverallAbsencePrimary(current: "6.10", previous: "6.90", previous2: "5.45")));

        var response = await _sut.Execute(Request("100001"));

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
    public async Task Absence_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("XYZ")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x.WithOverallAbsencePrimary(current: "71", previous: "70", previous2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.Absence.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task FilterBy_IgnoresInvalidFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "18", previous: "75", previous2: "80")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["xxx"] = "1",
            [Absence.Filters.Type.Key] = Absence.Filters.Type.Values.Overall,
            ["yyy"] = "2",
        }));

        response.Absence.Series.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Absence_FilterBy_Type_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues()
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

        var response = await _sut.Execute(Request("100001", filterBy: new()
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
    public async Task Absence_FilterBy_Type_ContainsYearByYearValuesForSelectedType(string type, double[] currentSchool, double[] la, double[] england)
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

        var response = await _sut.Execute(Request("100001", filterBy: new()
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

    private GetSchoolAttendanceMeasuresRequest Request(string urn, Dictionary<string, string>? filterBy = null) =>
            new(urn, filterBy ?? []);
}