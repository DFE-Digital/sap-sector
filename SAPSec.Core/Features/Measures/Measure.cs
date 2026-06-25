using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Primary;

namespace SAPSec.Core.Features.Measures;

public record Measure(
    string Key,
    string Name,
    MeasureDataType DataType,
    IEnumerable<MeasureAvailableFilter> Filters,
    IEnumerable<MeasureSeries> Series,
    IEnumerable<TopPerformer>? TopPerformers = null)
{
    internal static Measure ForSecondarySchool<T>(
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
            MeasureSeries.ForSecondarySchool(currentSchool, similarSchools, fieldSelector),
            TopPerformer.ForSecondarySchool(currentSchool, similarSchools, fieldSelector));
    }

    internal static Measure ForSecondarySchoolComparison<T>(
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
            MeasureSeries.ForSecondarySchoolComparison(currentSchool, similarSchool, fieldSelector));
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