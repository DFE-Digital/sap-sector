using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using System.Globalization;

namespace SAPSec.Core.Features.Attendance.UseCases;

public class GetAttendanceMeasures(
    IAbsenceRepository repository,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetAttendanceMeasuresResponse> Execute(GetAttendanceMeasuresRequest request)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(request.Urn);
        if (establishment is null)
        {
            throw new NotFoundException($"School with URN {request.Urn} was not found");
        }

        var data = await repository.GetByUrnAsync(request.Urn);
        var similarSchoolUrns = (await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.Urn))
            .Select(x => x.NeighbourURN)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var similarSchoolData = similarSchoolUrns.Length == 0
            ? Array.Empty<AbsenceData>()
            : await repository.GetByUrnsAsync(similarSchoolUrns);

        var overallSchoolSeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct));
        var persistentSchoolSeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct));

        var overallLocalAuthoritySeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Previous2_Pct));
        var persistentLocalAuthoritySeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous2_Pct));

        var overallEnglandSeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Previous2_Pct));
        var persistentEnglandSeries = new AttendanceMeasureSeries(
            ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Previous2_Pct));
        var overallSimilarSchoolsSeries = new AttendanceMeasureSeries(
            AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct))),
            AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct))),
            AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct))));
        var persistentSimilarSchoolsSeries = new AttendanceMeasureSeries(
            AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct))),
            AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct))),
            AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct))));

        return new(
            new AttendanceMeasureAverage(
                Average(overallSchoolSeries.Current, overallSchoolSeries.Previous, overallSchoolSeries.Previous2),
                AverageAvailable(similarSchoolData.Select(x => Average(
                    ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct),
                    ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct),
                    ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct)))),
                Average(overallLocalAuthoritySeries.Current, overallLocalAuthoritySeries.Previous, overallLocalAuthoritySeries.Previous2),
                Average(overallEnglandSeries.Current, overallEnglandSeries.Previous, overallEnglandSeries.Previous2)),
            new AttendanceMeasureYearByYear(
                overallSchoolSeries,
                overallSimilarSchoolsSeries,
                overallLocalAuthoritySeries,
                overallEnglandSeries),
            new AttendanceMeasureAverage(
                Average(persistentSchoolSeries.Current, persistentSchoolSeries.Previous, persistentSchoolSeries.Previous2),
                AverageAvailable(similarSchoolData.Select(x => Average(
                    ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct),
                    ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct),
                    ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct)))),
                Average(persistentLocalAuthoritySeries.Current, persistentLocalAuthoritySeries.Previous, persistentLocalAuthoritySeries.Previous2),
                Average(persistentEnglandSeries.Current, persistentEnglandSeries.Previous, persistentEnglandSeries.Previous2)),
            new AttendanceMeasureYearByYear(
                persistentSchoolSeries,
                persistentSimilarSchoolsSeries,
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

    private static decimal? AverageAvailable(IEnumerable<decimal?> values)
    {
        var availableValues = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return availableValues.Count == 0
            ? null
            : Math.Round(availableValues.Average(), 2, MidpointRounding.AwayFromZero);
    }
}

public record GetAttendanceMeasuresRequest(string Urn);

public record AttendanceMeasureAverage(
    decimal? SchoolValue,
    decimal? SimilarSchoolsValue,
    decimal? LocalAuthorityValue,
    decimal? EnglandValue);

public record AttendanceMeasureSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);

public record AttendanceMeasureYearByYear(
    AttendanceMeasureSeries School,
    AttendanceMeasureSeries SimilarSchools,
    AttendanceMeasureSeries LocalAuthority,
    AttendanceMeasureSeries England);

public record GetAttendanceMeasuresResponse(
    AttendanceMeasureAverage OverallAbsenceThreeYearAverage,
    AttendanceMeasureYearByYear OverallAbsenceYearByYear,
    AttendanceMeasureAverage PersistentAbsenceThreeYearAverage,
    AttendanceMeasureYearByYear PersistentAbsenceYearByYear);
