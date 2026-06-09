using SAPSec.Core.Filtering;
using SAPSec.Core.School.Secondary;

namespace SAPSec.Core.Measures;

public record Measure(
    string Key,
    string Name,
    MeasureDataType DataType,
    IEnumerable<MeasureAvailableFilter> Filters,
    IEnumerable<SubMeasure> SubMeasures)
{
    internal static Measure ForSecondarySchool<T>(
        string key,
        string name,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SecondarySchoolData<T> currentSchool,
        IEnumerable<SecondarySchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector)
    {
        return new Measure(
            key,
            name,
            dataType,
            availableFilters.ToList(),
            [
                ThreeYearAverageSubMeasure.ForSecondarySchool(currentSchool, similarSchools, fieldSelector),
                TopPerformersSubMeasure.ForSecondarySchool(currentSchool, similarSchools, fieldSelector),
                YearByYearSubMeasure.ForSecondarySchool(currentSchool, similarSchools, fieldSelector)
            ]);
    }

    internal static Measure ForSecondarySchoolComparison<T>(
        string key,
        string name,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SecondarySchoolData<T> currentSchool,
        SecondarySchoolData<T> similarSchool,
        IEnumerable<SecondarySchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector)
    {
        return new Measure(
            key,
            name,
            dataType,
            availableFilters.ToList(),
            [
                ThreeYearAverageSubMeasure.ForSecondarySchoolComparison(currentSchool, similarSchool, fieldSelector),
                YearByYearSubMeasure.ForSecondarySchoolComparison(currentSchool, similarSchool, fieldSelector)
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