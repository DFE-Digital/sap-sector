using SAPSec.Core.Collections;
using SAPSec.Core.Filtering;
using SAPSec.Core.Text;
using SAPSec.Data.Repositories;
using static SAPSec.Core.Features.Measures.Measures.Primary;

namespace SAPSec.Core.Features.Measures.Primary;

/// <summary>
/// KS2 progress scores in reading, writing and maths, filtered by subject (default reading).
/// Scores can be positive or negative; England is always around 0.
/// </summary>
internal static class Ks2ProgressScores
{
    // Same latest year as the other KS2 measures (2024 to 2025).
    private const int Year = 2024;

    public static Measure ForSchool(
        SchoolMeasureData<Ks2PerformanceData> currentSchool,
        IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools,
        CaseInsensitiveDictionary<string> filters,
        bool useEarliestYearAsLatest = false)
    {
        var (availableFilters, fieldSelector) = ResolveFilters(filters, useEarliestYearAsLatest);

        return Measure.ForSchool(
            Ks2Progress.Key,
            Ks2Progress.Name,
            Year,
            MeasureDataType.ProgressScore,
            availableFilters,
            currentSchool,
            similarSchools,
            fieldSelector);
    }

    public static Measure ForSchoolComparison(
        SchoolMeasureData<Ks2PerformanceData> currentSchool,
        SchoolMeasureData<Ks2PerformanceData> similarSchool,
        CaseInsensitiveDictionary<string> filters,
        bool useEarliestYearAsLatest = false)
    {
        var (availableFilters, fieldSelector) = ResolveFilters(filters, useEarliestYearAsLatest);

        return Measure.ForSchoolComparison(
            Ks2Progress.Key,
            Ks2Progress.Name,
            Year,
            MeasureDataType.ProgressScore,
            availableFilters,
            currentSchool,
            similarSchool,
            fieldSelector);
    }

    /// <summary>
    /// The 'Number of pupils' figures for a school and the selected subject: eligible pupils (the same for
    /// every subject) and the proportion of pupils included in that subject's progress measure.
    /// Only 'all pupils' figures for now; the pupil characteristic filter is a separate ticket.
    /// </summary>
    public static SchoolPupilNumbers PupilNumbers(string urn, Ks2PerformanceData? data, CaseInsensitiveDictionary<string> filters)
    {
        var school = data?.EstablishmentPerformance;
        var subject = SelectedSubject(filters);

        var included = subject switch
        {
            _ when subject.EqualsCaseInsensitive(Ks2Progress.Filters.Subject.Values.Writing) => (
                school?.WritingIncluded_Tot_Cohort_Est_Previous2_Num,
                school?.WritingIncluded_Tot_Cohort_Est_Previous_Num,
                school?.WritingIncluded_Tot_Cohort_Est_Current_Num),
            _ when subject.EqualsCaseInsensitive(Ks2Progress.Filters.Subject.Values.Maths) => (
                school?.MathsIncluded_Tot_Cohort_Est_Previous2_Num,
                school?.MathsIncluded_Tot_Cohort_Est_Previous_Num,
                school?.MathsIncluded_Tot_Cohort_Est_Current_Num),
            _ => (
                school?.ReadingIncluded_Tot_Cohort_Est_Previous2_Num,
                school?.ReadingIncluded_Tot_Cohort_Est_Previous_Num,
                school?.ReadingIncluded_Tot_Cohort_Est_Current_Num)
        };

        return new SchoolPupilNumbers(
            urn,
            new PupilNumbersYears<int>(
                ParseCount(school?.EligiblePupils_Tot_Cohort_Est_Previous2_Num),
                ParseCount(school?.EligiblePupils_Tot_Cohort_Est_Previous_Num),
                ParseCount(school?.EligiblePupils_Tot_Cohort_Est_Current_Num)),
            new PupilNumbersYears<decimal>(
                MeasureHelper.ParseNullableDecimal(included.Item1),
                MeasureHelper.ParseNullableDecimal(included.Item2),
                MeasureHelper.ParseNullableDecimal(included.Item3)));
    }

    private static string SelectedSubject(CaseInsensitiveDictionary<string> filters) =>
        filters.ContainsKey(Ks2Progress.Filters.Subject.Key)
        && Ks2Progress.Filters.Subject.Values.AllValues.Any(v => v.Value.EqualsCaseInsensitive(filters[Ks2Progress.Filters.Subject.Key]))
            ? filters[Ks2Progress.Filters.Subject.Key]
            : Ks2Progress.Filters.Subject.Values.Reading;

    private static int? ParseCount(string? value)
    {
        var parsed = MeasureHelper.ParseNullableDecimal(value);
        return parsed.HasValue ? (int)Math.Round(parsed.Value, MidpointRounding.AwayFromZero) : null;
    }

    private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(
        CaseInsensitiveDictionary<string> filters,
        bool useEarliestYearAsLatest)
    {
        var subject = SelectedSubject(filters);

        IEnumerable<MeasureAvailableFilter> availableFilters = [
            new MeasureAvailableFilter(
                Ks2Progress.Filters.Subject.Key,
                Ks2Progress.Filters.Subject.Name,
                Ks2Progress.Filters.Subject.Values.AllValues.Select(f =>
                    new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(subject)))
                .ToList())
        ];

        MeasureFieldSelector<Ks2PerformanceData> fieldSelector = subject switch
        {
            _ when subject.EqualsCaseInsensitive(Ks2Progress.Filters.Subject.Values.Writing) => new(
                x => x?.EstablishmentPerformance?.WritingProgress_Tot_Cohort_Est_Current_Num,
                x => x?.EstablishmentPerformance?.WritingProgress_Tot_Cohort_Est_Previous_Num,
                x => x?.EstablishmentPerformance?.WritingProgress_Tot_Cohort_Est_Previous2_Num,
                x => x?.LocalAuthorityPerformance?.WritingProgress_Tot_Cohort_LA_Current_Num,
                x => x?.LocalAuthorityPerformance?.WritingProgress_Tot_Cohort_LA_Previous_Num,
                x => x?.LocalAuthorityPerformance?.WritingProgress_Tot_Cohort_LA_Previous2_Num,
                x => x?.EnglandPerformance?.WritingProgress_Tot_Cohort_Eng_Current_Num,
                x => x?.EnglandPerformance?.WritingProgress_Tot_Cohort_Eng_Previous_Num,
                x => x?.EnglandPerformance?.WritingProgress_Tot_Cohort_Eng_Previous2_Num),
            _ when subject.EqualsCaseInsensitive(Ks2Progress.Filters.Subject.Values.Maths) => new(
                x => x?.EstablishmentPerformance?.MathsProgress_Tot_Cohort_Est_Current_Num,
                x => x?.EstablishmentPerformance?.MathsProgress_Tot_Cohort_Est_Previous_Num,
                x => x?.EstablishmentPerformance?.MathsProgress_Tot_Cohort_Est_Previous2_Num,
                x => x?.LocalAuthorityPerformance?.MathsProgress_Tot_Cohort_LA_Current_Num,
                x => x?.LocalAuthorityPerformance?.MathsProgress_Tot_Cohort_LA_Previous_Num,
                x => x?.LocalAuthorityPerformance?.MathsProgress_Tot_Cohort_LA_Previous2_Num,
                x => x?.EnglandPerformance?.MathsProgress_Tot_Cohort_Eng_Current_Num,
                x => x?.EnglandPerformance?.MathsProgress_Tot_Cohort_Eng_Previous_Num,
                x => x?.EnglandPerformance?.MathsProgress_Tot_Cohort_Eng_Previous2_Num),
            _ => new(
                x => x?.EstablishmentPerformance?.ReadingProgress_Tot_Cohort_Est_Current_Num,
                x => x?.EstablishmentPerformance?.ReadingProgress_Tot_Cohort_Est_Previous_Num,
                x => x?.EstablishmentPerformance?.ReadingProgress_Tot_Cohort_Est_Previous2_Num,
                x => x?.LocalAuthorityPerformance?.ReadingProgress_Tot_Cohort_LA_Current_Num,
                x => x?.LocalAuthorityPerformance?.ReadingProgress_Tot_Cohort_LA_Previous_Num,
                x => x?.LocalAuthorityPerformance?.ReadingProgress_Tot_Cohort_LA_Previous2_Num,
                x => x?.EnglandPerformance?.ReadingProgress_Tot_Cohort_Eng_Current_Num,
                x => x?.EnglandPerformance?.ReadingProgress_Tot_Cohort_Eng_Previous_Num,
                x => x?.EnglandPerformance?.ReadingProgress_Tot_Cohort_Eng_Previous2_Num)
        };

        // TESTING ONLY (see Flags.Ks2ProgressUseEarliestYearAsLatest): show the 2022 to 2023 scores as the
        // latest year until 2025 to 2026 data is available. The other two years are unchanged.
        if (useEarliestYearAsLatest)
        {
            fieldSelector = fieldSelector with
            {
                SchoolCurrent = fieldSelector.SchoolPrevious2,
                LocalAuthorityCurrent = fieldSelector.LocalAuthorityPrevious2,
                EnglandCurrent = fieldSelector.EnglandPrevious2
            };
        }

        return (availableFilters, fieldSelector);
    }
}
