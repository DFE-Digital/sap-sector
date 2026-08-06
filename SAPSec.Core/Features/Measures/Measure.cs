using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools;

namespace SAPSec.Core.Features.Measures;

public record Measure(
    string Key,
    string Name,
    MeasureDataType DataType,
    IReadOnlyCollection<MeasureAvailableFilter> Filters,
    IReadOnlyCollection<MeasureSeries> Series,
    IReadOnlyCollection<TopPerformer>? TopPerformers = null)
{
    internal static Measure ForSchool<T>(
        string key,
        string name,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolData<T> currentSchool,
        IEnumerable<SchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector)
    {
        return new Measure(
            key,
            name,
            dataType,
            availableFilters.ToList(),
            MeasureSeries.ForSchool(currentSchool, similarSchools, fieldSelector),
            TopPerformer.BuildTopPerformers(currentSchool, similarSchools, fieldSelector, dataType));
    }

    internal static Measure ForSchoolComparison<T>(
        string key,
        string name,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolData<T> currentSchool,
        SchoolData<T> similarSchool,
        IEnumerable<SchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector)
    {
        return new Measure(
            key,
            name,
            dataType,
            availableFilters.ToList(),
            MeasureSeries.ForSchoolComparison(currentSchool, similarSchool, fieldSelector));
    }
}

public enum MeasureDataType
{
    Score,
    ScaledScore,
    GradePercentage,
    AbsencePercentage
}

public record MeasureAvailableFilter(
    string Key,
    string Name,
    IReadOnlyCollection<FilterOption> Options);

internal record MeasureFieldSelector<T>(
    Func<T?, string?> SchoolCurrent,
    Func<T?, string?> SchoolPrevious,
    Func<T?, string?> SchoolPrevious2,
    Func<T?, string?> LocalAuthorityCurrent,
    Func<T?, string?> LocalAuthorityPrevious,
    Func<T?, string?> LocalAuthorityPrevious2,
    Func<T?, string?> EnglandCurrent,
    Func<T?, string?> EnglandPrevious,
    Func<T?, string?> EnglandPrevious2);
