using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Test.Common.Builders;
using static SAPSec.Core.Features.Measures.Measures;

namespace SAPSec.Core.Tests.Features.Measures.Attendance;

public partial class GetSchoolAttendanceMeasuresUseCaseTests
{
    [Theory]
    [InlineData(MeasurePhase.Primary)]
    [InlineData(MeasurePhase.Secondary)]
    public async Task Absence_WithoutPhaseScopedKeys_UsesUnprefixedKeys(MeasurePhase phase)
    {
        SetupAllThroughSchool();

        var response = await _sut.Execute(Request(phase, "100001"));

        response.Absence.Key.Should().Be("absence");
        response.Absence.Filters.Select(f => f.Key).Should().Equal(
            "absence-type",
            "absence-characteristic");
    }

    [Theory]
    [InlineData(MeasurePhase.Primary, "primary-")]
    [InlineData(MeasurePhase.Secondary, "secondary-")]
    public async Task Absence_WithPhaseScopedKeys_PrefixesMeasureAndFilterKeys(MeasurePhase phase, string prefix)
    {
        SetupAllThroughSchool();

        var response = await _sut.Execute(ScopedRequest(phase, "100001"));

        response.Absence.Key.Should().Be($"{prefix}absence");
        response.Absence.Filters.Select(f => f.Key).Should().Equal(
            $"{prefix}absence-type",
            $"{prefix}absence-characteristic");
    }

    [Theory]
    [InlineData(MeasurePhase.Primary, "primary-absence-type", MeasureDataType.PersistentAbsencePercentage)]
    [InlineData(MeasurePhase.Secondary, "primary-absence-type", MeasureDataType.OverallAbsencePercentage)]
    [InlineData(MeasurePhase.Secondary, "secondary-absence-type", MeasureDataType.PersistentAbsencePercentage)]
    [InlineData(MeasurePhase.Primary, "secondary-absence-type", MeasureDataType.OverallAbsencePercentage)]
    public async Task Absence_WithPhaseScopedKeys_OnlyReadsFiltersForItsOwnPhase(
        MeasurePhase phase,
        string filterKey,
        MeasureDataType expectedDataType)
    {
        SetupAllThroughSchool();

        var response = await _sut.Execute(ScopedRequest(
            phase,
            "100001",
            new() { [filterKey] = Absence.Filters.Type.Values.Persistent }));

        response.Absence.DataType.Should().Be(expectedDataType);
    }

    [Theory]
    [InlineData(MeasurePhase.Primary)]
    [InlineData(MeasurePhase.Secondary)]
    public async Task Absence_WithoutPhaseScopedKeys_IgnoresPhaseScopedFilters(MeasurePhase phase)
    {
        SetupAllThroughSchool();

        var response = await _sut.Execute(Request(
            phase,
            "100001",
            new()
            {
                ["primary-absence-type"] = Absence.Filters.Type.Values.Persistent,
                ["secondary-absence-type"] = Absence.Filters.Type.Values.Persistent
            }));

        response.Absence.DataType.Should().Be(MeasureDataType.OverallAbsencePercentage);
    }

    [Theory]
    [InlineData(MeasurePhase.Primary, 5.12, 4.83)]
    [InlineData(MeasurePhase.Secondary, 6.57, 6.11)]
    public async Task Absence_WithPhaseScopedKeys_UsesPhaseSpecificLAAndEnglandValues(
        MeasurePhase phase,
        double expectedLa,
        double expectedEngland)
    {
        SetupAllThroughSchool();

        _absenceRepo.SetupLAAbsence(
            Build.Absence.LA("001", x => x
                .WithOverallAbsencePrimary(current: "5.12", previous: "5.00", previous2: "4.90")
                .WithOverallAbsenceSecondary(current: "6.57", previous: "6.40", previous2: "6.30")));

        _absenceRepo.SetupEnglandAbsence(
            Build.Absence.England(x => x
                .WithOverallAbsencePrimary(current: "4.83", previous: "4.70", previous2: "4.60")
                .WithOverallAbsenceSecondary(current: "6.11", previous: "6.00", previous2: "5.90")));

        var response = await _sut.Execute(ScopedRequest(phase, "100001"));

        response.Absence.Series.Single(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage)
            .Current.Should().Be((decimal)expectedLa);
        response.Absence.Series.Single(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage)
            .Current.Should().Be((decimal)expectedEngland);
    }

    [Fact]
    public async Task Absence_WithPhaseScopedKeys_CurrentSchoolSeriesIsTheSameForBothPhases()
    {
        SetupAllThroughSchool();

        _absenceRepo.SetupEstablishmentAbsence(
            Build.Absence.Establishment("100001", x => x.WithOverallAbsence(current: "8.00", previous: "8.05", previous2: "7.91")));

        var primary = await _sut.Execute(ScopedRequest(MeasurePhase.Primary, "100001"));
        var secondary = await _sut.Execute(ScopedRequest(MeasurePhase.Secondary, "100001"));

        var primarySchool = primary.Absence.Series.Single(s => s.SeriesType == MeasureSeriesType.CurrentSchool);
        var secondarySchool = secondary.Absence.Series.Single(s => s.SeriesType == MeasureSeriesType.CurrentSchool);

        primarySchool.Should().Be(secondarySchool);
        primarySchool.Current.Should().Be(8.00m);
    }

    private void SetupAllThroughSchool() =>
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.AllThrough().InLA("001")));

    private static GetSchoolAttendanceMeasuresRequest ScopedRequest(
        MeasurePhase phase,
        string urn,
        Dictionary<string, string>? filterBy = null) =>
            new(phase, urn, filterBy ?? [], ScopeKeysToPhase: true);
}
