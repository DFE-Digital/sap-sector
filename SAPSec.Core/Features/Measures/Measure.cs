using SAPSec.Core.Features.Filtering;
using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Measures;

public record Measure(
    string Key,
    string Name,
    MeasureDataType DataType,
    IEnumerable<MeasureAvailableFilter> Filters,
    IEnumerable<SubMeasure> SubMeasures)
{
    internal static Measure ForSchool(
        string key,
        string name,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolData schoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector)
    {
        return new Measure(
            key,
            name,
            dataType,
            availableFilters.ToList(),
            [
                ThreeYearAverageSubMeasure.ForSchool(schoolData, similarSchools, fieldSelector),
                TopPerformersSubMeasure.ForSchool(schoolData, similarSchools, fieldSelector),
                YearByYearSubMeasure.ForSchool(schoolData, similarSchools, fieldSelector)
            ]);
    }

    internal static Measure ForSchoolComparison(
        string key,
        string name,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolData currentSchoolData,
        SchoolData similarSchoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector)
    {
        return new Measure(
            key,
            name,
            dataType,
            availableFilters.ToList(),
            [
                ThreeYearAverageSubMeasure.ForSchoolComparison(currentSchoolData, similarSchoolData, fieldSelector),
                    YearByYearSubMeasure.ForSchoolComparison(currentSchoolData, similarSchoolData, fieldSelector)
            ]);
    }
}

public enum MeasureDataType
{
    Score,
    GradePercentage,
    AbsencePercentage
}

public record MeasureAvailableFilter(
    string Key,
    string Name,
    IReadOnlyCollection<FilterOption> Options);

internal record MeasureFieldSelector(
    Func<SchoolData?, string?> SchoolCurrent,
    Func<SchoolData?, string?> SchoolPrevious,
    Func<SchoolData?, string?> SchoolPrevious2,
    Func<SchoolData?, string?> LocalAuthorityCurrent,
    Func<SchoolData?, string?> LocalAuthorityPrevious,
    Func<SchoolData?, string?> LocalAuthorityPrevious2,
    Func<SchoolData?, string?> EnglandCurrent,
    Func<SchoolData?, string?> EnglandPrevious,
    Func<SchoolData?, string?> EnglandPrevious2);

internal sealed record SchoolData(
    string Urn,
    string Name,
    Ks4PerformanceData? PerformanceData,
    Ks4DestinationsData? DestinationsData);
