using SAPSec.Core.Features.Filtering;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public record Measure(
    string Key,
    string Name,
    int Year,
    MeasureDataType DataType,
    IReadOnlyCollection<MeasureAvailableFilter> Filters,
    IReadOnlyCollection<MeasureSeries> Series,
    IReadOnlyCollection<TopPerformer>? TopPerformers = null)
{
    internal static Measure ForSchool<T>(
        string key,
        string name,
        int year,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolMeasureData<T> currentSchool,
        IEnumerable<SchoolMeasureData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector)
        where T : class, IMeasureData
    {
        return new Measure(
            key,
            name,
            year,
            dataType,
            availableFilters.ToList(),
            MeasureSeries.ForSchool(currentSchool, similarSchools, fieldSelector),
            TopPerformer.BuildTopPerformers(currentSchool, similarSchools, fieldSelector, dataType));
    }

    internal static Measure ForSchoolAttendance<T>(
        string key,
        string name,
        int year,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolMeasureData<T> currentSchool,
        MeasureFieldSelector<T> fieldSelector)
        where T : class, IMeasureData
    {
        return new Measure(
            key,
            name,
            year,
            dataType,
            availableFilters.ToList(),
            MeasureSeries.ForSchoolAttendance(currentSchool, fieldSelector));
    }

    internal static Measure ForSchoolComparison<T>(
        string key,
        string name,
        int year,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolMeasureData<T> currentSchool,
        SchoolMeasureData<T> similarSchool,
        MeasureFieldSelector<T> fieldSelector)
        where T : class, IMeasureData
    {
        return new Measure(
            key,
            name,
            year,
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
    OverallAbsencePercentage,
    PersistentAbsencePercentage
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
