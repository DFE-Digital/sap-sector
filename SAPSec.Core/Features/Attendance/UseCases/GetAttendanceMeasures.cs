using SAPSec.Core;
using SAPSec.Core.Interfaces.Repositories;

namespace SAPSec.Core.Features.Attendance.UseCases;

public class GetAttendanceMeasures(
    IAttendanceRepository repository,
    IEstablishmentRepository establishmentRepository)
{
    public async Task<GetAttendanceMeasuresResponse> Execute(GetAttendanceMeasuresRequest request)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(request.Urn);
        if (establishment is null)
        {
            throw new NotFoundException($"School with URN {request.Urn} was not found");
        }

        var data = await repository.GetByUrnAsync(request.Urn);

        var overallSchoolSeries = new AttendanceMeasureSeries(
            data?.EstablishmentAttendance?.Abs_Tot_Est_Current_Pct,
            data?.EstablishmentAttendance?.Abs_Tot_Est_Previous_Pct,
            data?.EstablishmentAttendance?.Abs_Tot_Est_Previous2_Pct);
        var persistentSchoolSeries = new AttendanceMeasureSeries(
            data?.EstablishmentAttendance?.Abs_Persistent_Est_Current_Pct,
            data?.EstablishmentAttendance?.Abs_Persistent_Est_Previous_Pct,
            data?.EstablishmentAttendance?.Abs_Persistent_Est_Previous2_Pct);

        var overallEnglandSeries = new AttendanceMeasureSeries(
            data?.EnglandAttendance?.Abs_Tot_Eng_Current_Pct,
            data?.EnglandAttendance?.Abs_Tot_Eng_Previous_Pct,
            data?.EnglandAttendance?.Abs_Tot_Eng_Previous2_Pct);
        var persistentEnglandSeries = new AttendanceMeasureSeries(
            data?.EnglandAttendance?.Abs_Persistent_Eng_Current_Pct,
            data?.EnglandAttendance?.Abs_Persistent_Eng_Previous_Pct,
            data?.EnglandAttendance?.Abs_Persistent_Eng_Previous2_Pct);

        return new(
            new AttendanceMeasureAverage(
                Average(overallSchoolSeries.Current, overallSchoolSeries.Previous, overallSchoolSeries.Previous2),
                Average(overallEnglandSeries.Current, overallEnglandSeries.Previous, overallEnglandSeries.Previous2)),
            new AttendanceMeasureYearByYear(
                overallSchoolSeries,
                overallEnglandSeries),
            new AttendanceMeasureAverage(
                Average(persistentSchoolSeries.Current, persistentSchoolSeries.Previous, persistentSchoolSeries.Previous2),
                Average(persistentEnglandSeries.Current, persistentEnglandSeries.Previous, persistentEnglandSeries.Previous2)),
            new AttendanceMeasureYearByYear(
                persistentSchoolSeries,
                persistentEnglandSeries));
    }

    private static decimal? Average(params decimal?[] values)
    {
        var availableValues = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return availableValues.Count == 0
            ? null
            : Math.Round(availableValues.Average(), 1, MidpointRounding.AwayFromZero);
    }
}

public record GetAttendanceMeasuresRequest(string Urn);

public record AttendanceMeasureAverage(
    decimal? SchoolValue,
    decimal? EnglandValue);

public record AttendanceMeasureSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);

public record AttendanceMeasureYearByYear(
    AttendanceMeasureSeries School,
    AttendanceMeasureSeries England);

public record GetAttendanceMeasuresResponse(
    AttendanceMeasureAverage OverallAbsenceThreeYearAverage,
    AttendanceMeasureYearByYear OverallAbsenceYearByYear,
    AttendanceMeasureAverage PersistentAbsenceThreeYearAverage,
    AttendanceMeasureYearByYear PersistentAbsenceYearByYear);
