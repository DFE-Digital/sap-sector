using SAPSec.Core.Interfaces.Repositories;
using System.Globalization;

namespace SAPSec.Core.Features.Attendance.UseCases;

public class GetAttendanceMeasures(
    IAbsenceRepository repository,
    IEstablishmentRepository establishmentRepository)
{
    public async Task<GetAttendanceMeasuresResponse> Execute(GetAttendanceMeasuresRequest request)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(request.Urn);
        if (establishment is null)
        {
            throw new NotFoundException($"School with URN {request.Urn} was not found");
        }

        var data = await repository.GetByUrnAsync(request.Urn, establishment.LAId);

        var overallSchoolSeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentAttendance?.Abs_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentAttendance?.Abs_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentAttendance?.Abs_Tot_Est_Previous2_Pct));
        var persistentSchoolSeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentAttendance?.Abs_Persistent_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentAttendance?.Abs_Persistent_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentAttendance?.Abs_Persistent_Est_Previous2_Pct));

        var overallLocalAuthoritySeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityAttendance?.Abs_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityAttendance?.Abs_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityAttendance?.Abs_Tot_LA_Previous2_Pct));
        var persistentLocalAuthoritySeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityAttendance?.Abs_Persistent_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityAttendance?.Abs_Persistent_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityAttendance?.Abs_Persistent_LA_Previous2_Pct));

        var overallEnglandSeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.EnglandAttendance?.Abs_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandAttendance?.Abs_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandAttendance?.Abs_Tot_Eng_Previous2_Pct));
        var persistentEnglandSeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.EnglandAttendance?.Abs_Persistent_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandAttendance?.Abs_Persistent_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandAttendance?.Abs_Persistent_Eng_Previous2_Pct));

        return new(
            new AttendanceMeasureAverage(
                Average(overallSchoolSeries.Current, overallSchoolSeries.Previous, overallSchoolSeries.Previous2),
                Average(overallLocalAuthoritySeries.Current, overallLocalAuthoritySeries.Previous, overallLocalAuthoritySeries.Previous2),
                Average(overallEnglandSeries.Current, overallEnglandSeries.Previous, overallEnglandSeries.Previous2)),
            new AttendanceMeasureYearByYear(
                overallSchoolSeries,
                overallLocalAuthoritySeries,
                overallEnglandSeries),
            new AttendanceMeasureAverage(
                Average(persistentSchoolSeries.Current, persistentSchoolSeries.Previous, persistentSchoolSeries.Previous2),
                Average(persistentLocalAuthoritySeries.Current, persistentLocalAuthoritySeries.Previous, persistentLocalAuthoritySeries.Previous2),
                Average(persistentEnglandSeries.Current, persistentEnglandSeries.Previous, persistentEnglandSeries.Previous2)),
            new AttendanceMeasureYearByYear(
                persistentSchoolSeries,
                persistentLocalAuthoritySeries,
                persistentEnglandSeries));
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static decimal? Average(params decimal?[] values)
    {
        if (values.Any(v => !v.HasValue))
        {
            return null;
        }

        return Math.Round(values.Average(v => v!.Value), 2, MidpointRounding.AwayFromZero);
    }
}

public record GetAttendanceMeasuresRequest(string Urn);

public record AttendanceMeasureAverage(
    decimal? SchoolValue,
    decimal? LocalAuthorityValue,
    decimal? EnglandValue);

public record AttendanceMeasureSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);

public record AttendanceMeasureYearByYear(
    AttendanceMeasureSeries School,
    AttendanceMeasureSeries LocalAuthority,
    AttendanceMeasureSeries England);

public record GetAttendanceMeasuresResponse(
    AttendanceMeasureAverage OverallAbsenceThreeYearAverage,
    AttendanceMeasureYearByYear OverallAbsenceYearByYear,
    AttendanceMeasureAverage PersistentAbsenceThreeYearAverage,
    AttendanceMeasureYearByYear PersistentAbsenceYearByYear);
