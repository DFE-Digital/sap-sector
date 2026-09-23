namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// KS2 performance measures: reading, writing and maths, grammar, punctuation and spelling, scaled scores and science.
/// </summary>
/// <remarks>
/// To roll to a new year, bump <see cref="DataYears"/> and point the sources at the new files.
/// Property names: {metric}_[{subject}_]{breakdown}_Cohort_{scope}_{period}_Num.
/// </remarks>
public static class Ks2Performance
{
    public const string Type = "KS2_Performance";

    private const int CurrentYear = DataYears.Ks2Performance;
    private static readonly AcademicYear Current = new(CurrentYear);
    private static readonly AcademicYear Previous = new(CurrentYear - 1);
    private static readonly AcademicYear Previous2 = new(CurrentYear - 2);

    private const string NameTemplate = "{metric}_{breakdown}_Cohort_{scope}_{period}_{unit}";

    // Reading, writing and maths combined, then each subject on its own (named e.g. RwmExpected_Reading_Tot_Cohort_…).
    private static readonly (string NamePrefix, string Subject)[] RwmSubjects =
    [
        ("", "Reading, writing and maths"),
        ("Reading_", "Reading"),
        ("Writing_", "Writing"),
        ("Maths_", "Maths"),
    ];

    public static IReadOnlyList<MeasureSet> MeasureSets() => [Performance()];

    public static MeasureSet Performance()
    {
        var set = new MeasureSet(Type, "Performance")
            .Years(CurrentYear)
            .Source(Scope.England, Period.Current, Characteristics("geographic_level", "National", Current))
            .Source(Scope.England, Period.Previous, Characteristics("geographic_level", "National", Previous))
            .Source(Scope.England, Period.Previous2, Characteristics("geographic_level", "National", Previous2))
            .Source(Scope.LA, Period.Current, Characteristics("old_la_code", "Local authority", Current))
            .Source(Scope.LA, Period.Previous, Characteristics("old_la_code", "Local authority", Previous))
            .Source(Scope.LA, Period.Previous2, Characteristics("old_la_code", "Local authority", Previous2))
            .Source(Scope.Establishment, Period.Current, SchoolAttainment(Current))
            .Source(Scope.Establishment, Period.Previous, SchoolAttainment(Previous))
            .Source(Scope.Establishment, Period.Previous2, SchoolAttainment(Previous2))
            .Breakdowns(
                Breakdowns.Total, Breakdowns.Boys, Breakdowns.Girls, Breakdowns.Disadvantaged,
                Breakdowns.NotDisadvantaged, Breakdowns.Eal, Breakdowns.FirstLanguageEnglish, Breakdowns.NonMobile);

        foreach (var (name, field) in new[] { ("RwmExpected", "expected_standard_pupil_percent"), ("RwmHigher", "higher_standard_pupil_percent") })
        {
            foreach (var (prefix, subject) in RwmSubjects)
                set.Metric(Measure(name, field, subject).Named($"{{metric}}_{prefix}{{breakdown}}_Cohort_{{scope}}_{{period}}_{{unit}}"));
        }

        return set
            .Metric(Measure("ReadingScaledScore", "average_scaled_score", "Reading"))
            .Metric(Measure("MathsScaledScore", "average_scaled_score", "Maths"))
            .Metric(Measure("GpsExpected", "expected_standard_pupil_percent", "Grammar, punctuation and spelling"))
            .Metric(Measure("GpsHigher", "higher_standard_pupil_percent", "Grammar, punctuation and spelling"))
            .Metric(Measure("ScienceExpected", "expected_standard_pupil_percent", "Science").In(Scope.England, Scope.LA));
    }

    private static Metric Measure(string name, string field, string subject) =>
        new Metric(name, field).Named(NameTemplate).Where("subject", subject);

    /// <summary>
    /// EES KS2 regional, LA and pupil characteristics: every year in one file. Breakdowns select one pupil group,
    /// leaving the other characteristics at Total.
    /// </summary>
    private static Source Characteristics(string key, string geographicLevel, AcademicYear year) =>
        Source.Ees("ks2_regional_local_authority_and_pupil_characteristics_2019_to_2025_revised")
            .KeyedBy(key)
            .Where("geographic_level", geographicLevel)
            .Where("fsm_status", "Total")
            .Where("ethnicity_minor", "Total")
            .Where("sen_provision", "Total")
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total, ("sex", "Total"), ("disadvantage_status", "Total"), ("first_language", "Total"))
            .Provides(Breakdowns.Boys, ("sex", "Boys"), ("disadvantage_status", "Total"), ("first_language", "Total"))
            .Provides(Breakdowns.Girls, ("sex", "Girls"), ("disadvantage_status", "Total"), ("first_language", "Total"))
            .Provides(Breakdowns.Disadvantaged, ("sex", "Total"), ("disadvantage_status", "Disadvantaged"), ("first_language", "Total"))
            .Provides(Breakdowns.NotDisadvantaged, ("sex", "Total"), ("disadvantage_status", "Not known to be disadvantaged"), ("first_language", "Total"))
            .Provides(Breakdowns.Eal, ("sex", "Total"), ("disadvantage_status", "Total"), ("first_language", "Known or believed to be other than English"))
            .Provides(Breakdowns.FirstLanguageEnglish, ("sex", "Total"), ("disadvantage_status", "Total"), ("first_language", "Known or believed to be English"));

    /// <summary>EES KS2 school attainment: one row per school, subject and breakdown.</summary>
    private static Source SchoolAttainment(AcademicYear year) =>
        Source.Ees("ks2_school_attainment_data")
            .KeyedBy("school_urn")
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total, ("breakdown", "Total"))
            .Provides(Breakdowns.Boys, ("breakdown", "Boys"))
            .Provides(Breakdowns.Girls, ("breakdown", "Girls"))
            .Provides(Breakdowns.Disadvantaged, ("breakdown", "Disadvantaged"))
            .Provides(Breakdowns.NotDisadvantaged, ("breakdown", "Not known to be disadvantaged"))
            .Provides(Breakdowns.Eal, ("breakdown", "Known or believed to be other than English"))
            .Provides(Breakdowns.NonMobile, ("breakdown", "Non mobile"));
}
