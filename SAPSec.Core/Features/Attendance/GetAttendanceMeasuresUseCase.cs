using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Attendance;

public class GetAttendanceMeasuresUseCase(
    IAbsenceRepository attendanceRepository,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsSecondaryRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsPrimaryRepository)
    : IUseCase<GetAttendanceMeasuresRequest, GetAttendanceMeasuresResponse>
{
    public async Task<GetAttendanceMeasuresResponse> Execute(GetAttendanceMeasuresRequest request)
    {
        var dataProvider = new AttendanceMeasuresDataProvider(
              attendanceRepository,
              establishmentRepository,
              similarSchoolsPrimaryRepository,
              similarSchoolsSecondaryRepository);

        var (currentSchoolPerformance, similarSchoolsPerformance) = await dataProvider.GetSimilarSchoolsAttendance(request.Urn, request.Phase);

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolsPerformance.Count,
            AttendanceMeasures.TotalAbsence.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            AttendanceMeasures.PersistentAbsence.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy));
    }

}

public record GetAttendanceMeasuresRequest(
string Urn,
string Phase,
IDictionary<string, string>? FilterBy = null);

public record GetAttendanceMeasuresResponse(
SchoolInfo.SchoolInfo School,
int SimilarSchoolsCount,
Measure TotalAbsence,
Measure PersistentAbsence);

//    private static decimal? ParseNullableDecimal(string? value)
//    {
//        if (string.IsNullOrWhiteSpace(value))
//        {
//            return null;
//        }

//        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
//            ? parsed
//            : null;
//    }

//    private static decimal? Average(params decimal?[] values)
//    {
//        if (values.Any(v => !v.HasValue))
//        {
//            return null;
//        }

//        return Math.Round(values.Average(v => v!.Value), 2, MidpointRounding.AwayFromZero);
//    }

//    private static decimal? AverageAvailable(IEnumerable<decimal?> values)
//    {
//        var availableValues = values
//            .Where(v => v.HasValue)
//            .Select(v => v!.Value)
//            .ToList();

//        return availableValues.Count == 0
//            ? null
//            : Math.Round(availableValues.Average(), 2, MidpointRounding.AwayFromZero);
//    }

//    private static IReadOnlyList<AttendanceTopPerformer> BuildTopPerformers(
//        Establishment currentSchool,
//        decimal? currentSchoolValue,
//        IEnumerable<SimilarSchoolAttendanceMeasure> similarSchools,
//        Func<SimilarSchoolAttendanceMeasure, decimal?> selector)
//    {
//        var currentSchoolCandidate = new AttendanceTopPerformerCandidate(
//            currentSchool.URN,
//            currentSchool.EstablishmentName,
//            currentSchoolValue,
//            IsCurrentSchool: true);

//        return similarSchools
//            .Select(x => new AttendanceTopPerformerCandidate(x.Urn, x.Name, selector(x), IsCurrentSchool: false))
//            .Append(currentSchoolCandidate)
//            .Where(x => x.Value.HasValue)
//            .GroupBy(x => x.Urn, StringComparer.Ordinal)
//            .Select(x => x.OrderByDescending(candidate => candidate.IsCurrentSchool).First())
//            .OrderBy(x => x.Value)
//            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
//            .Take(3)
//            .Select((x, index) => new AttendanceTopPerformer(index + 1, x.Urn, x.Name, x.Value, x.IsCurrentSchool))
//            .ToList()
//            .AsReadOnly();
//    }

//    private sealed record AttendanceTopPerformerCandidate(
//        string Urn,
//        string Name,
//        decimal? Value,
//        bool IsCurrentSchool);




//public record AttendanceMeasureAverage(
//    decimal? SchoolValue,
//    decimal? SimilarSchoolsValue,
//    decimal? LocalAuthorityValue,
//    decimal? EnglandValue);

//public record AttendanceMeasureSeries(
//    decimal? Current,
//    decimal? Previous,
//    decimal? Previous2);

//public record AttendanceMeasureYearByYear(
//    AttendanceMeasureSeries School,
//    AttendanceMeasureSeries SimilarSchools,
//    AttendanceMeasureSeries LocalAuthority,
//    AttendanceMeasureSeries England);



////public record GetAttendanceMeasuresResponse(
////    AttendanceMeasureAverage OverallAbsenceThreeYearAverage,
////    IReadOnlyList<AttendanceTopPerformer> OverallAbsenceTopPerformers,
////    AttendanceMeasureYearByYear OverallAbsenceYearByYear,
////    AttendanceMeasureAverage PersistentAbsenceThreeYearAverage,
////    IReadOnlyList<AttendanceTopPerformer> PersistentAbsenceTopPerformers,
////    AttendanceMeasureYearByYear PersistentAbsenceYearByYear);

//public record AttendanceTopPerformer(
//    int Rank,
//    string Urn,
//    string Name,
//    decimal? Value,
//    bool IsCurrentSchool = false);

//internal sealed record SimilarSchoolAttendanceMeasure(
//    string Urn,
//    string Name,
//    AbsenceData? AbsenceData);
