using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Features.Measures.Measures;

namespace SAPSec.Core.Tests.Features.Measures.Attendance;

public class GetSchoolAllThroughAttendanceMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryAbsenceRepository _absenceRepo;
    private readonly GetSchoolAllThroughAttendanceMeasuresUseCase _sut;

    public GetSchoolAllThroughAttendanceMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _absenceRepo = new(_establishmentRepo);
        _sut = new GetSchoolAllThroughAttendanceMeasuresUseCase(
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
            Build.Establishment("100001", "Test School", x => x.AllThrough().InLA("001", "Test LA")));

        var response = await _sut.Execute(Request("100001"));

        response.School.Urn.Should().Be("100001");
        response.School.Name.Should().Be("Test School");
    }

    [Fact]
    public async Task PrimaryAbsence_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough()));

        var response = await _sut.Execute(Request("100001"));

        response.PrimaryAbsence.Series.Select(s => s.SeriesType).Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task SecondaryAbsence_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough()));

        var response = await _sut.Execute(Request("100001"));

        response.SecondaryAbsence.Series.Select(s => s.SeriesType).Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task PrimaryAndSecondaryAbsence_HaveDistinctKeys()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough()));

        var response = await _sut.Execute(Request("100001"));

        response.PrimaryAbsence.Key.Should().Be("primary-absence");
        response.SecondaryAbsence.Key.Should().Be("secondary-absence");
    }

    [Fact]
    public async Task PrimaryAndSecondaryAbsence_HaveDistinctFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough()));

        var response = await _sut.Execute(Request("100001"));

        response.PrimaryAbsence.Filters.Select(f => f.Key).Should().BeEquivalentTo([
            "primary-absence-type",
            "primary-absence-characteristic"
        ]);
        response.SecondaryAbsence.Filters.Select(f => f.Key).Should().BeEquivalentTo([
            "secondary-absence-type",
            "secondary-absence-characteristic"
        ]);
    }

    [Fact]
    public async Task CurrentSchoolSeries_IsTheSameForPrimaryAndSecondaryAbsence()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough().InLA("001")));

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")));

        var response = await _sut.Execute(Request("100001"));

        var primarySchoolSeries = response.PrimaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.CurrentSchool);
        var secondarySchoolSeries = response.SecondaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.CurrentSchool);

        primarySchoolSeries.Should().Be(secondarySchoolSeries);
        primarySchoolSeries.Current.Should().Be(8.00m);
    }

    [Fact]
    public async Task LAAndEnglandSeries_AreResolvedFromThePhaseSpecificFields()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough().InLA("001")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "4.83", previous: "4.70", previous2: "4.60")
                .WithOverallAbsenceSecondary(current: "6.11", previous: "6.00", previous2: "5.90")));

        var response = await _sut.Execute(Request("100001"));

        response.PrimaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage).Current.Should().Be(5.12m);
        response.PrimaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage).Current.Should().Be(4.83m);

        response.SecondaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage).Current.Should().Be(6.57m);
        response.SecondaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage).Current.Should().Be(6.11m);
    }

    [Fact]
    public async Task FilteringPrimaryAbsence_DoesNotAffectSecondaryAbsence()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough().InLA("001")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithPersistentAbsencePrimary(current: "20.00", previous: "19.50", previous2: "19.00")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")));

        var response = await _sut.Execute(Request(
            "100001",
            new Dictionary<string, string>
            {
                ["primary-absence-type"] = Absence.Filters.Type.Values.Persistent
            }));

        response.PrimaryAbsence.DataType.Should().Be(MeasureDataType.PersistentAbsencePercentage);
        response.PrimaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage).Current.Should().Be(20.00m);

        response.SecondaryAbsence.DataType.Should().Be(MeasureDataType.OverallAbsencePercentage);
        response.SecondaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage).Current.Should().Be(6.57m);
    }

    [Fact]
    public async Task FilteringSecondaryAbsence_DoesNotAffectPrimaryAbsence()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough().InLA("001")));

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")
                .WithPersistentAbsenceSecondary(current: "25.00", previous: "24.50", previous2: "24.00")));

        var response = await _sut.Execute(Request(
            "100001",
            new Dictionary<string, string>
            {
                ["secondary-absence-type"] = Absence.Filters.Type.Values.Persistent
            }));

        response.SecondaryAbsence.DataType.Should().Be(MeasureDataType.PersistentAbsencePercentage);
        response.SecondaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage).Current.Should().Be(25.00m);

        response.PrimaryAbsence.DataType.Should().Be(MeasureDataType.OverallAbsencePercentage);
        response.PrimaryAbsence.Series.Single(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage).Current.Should().Be(5.12m);
    }

    private static GetSchoolAllThroughAttendanceMeasuresRequest Request(
        string urn,
        Dictionary<string, string>? filterBy = null) =>
            new(urn, filterBy ?? []);
}
