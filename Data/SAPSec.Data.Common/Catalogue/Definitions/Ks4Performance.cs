using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// KS4 performance measures: headline measures, subject achievement bands and subject grade entries.
/// </summary>
/// <remarks>
/// To roll to a new year, bump <see cref="CurrentYear"/> and point each source at the new files.
/// Comments marked DATA ISSUE preserve datamap.csv behaviour that looks wrong; fix them in their own change.
/// </remarks>
public static class Ks4Performance
{
    public const string Type = "KS4_Performance";

    private const int CurrentYear = 2024;
    private static readonly AcademicYear Current = new(CurrentYear);
    private static readonly AcademicYear Previous = new(CurrentYear - 1);
    private static readonly AcademicYear Previous2 = new(CurrentYear - 2);

    private static readonly Subject[] Subjects =
    [
        new("Bio", "Biology", "RH3"),
        new("Chem", "Chemistry", "RD1"),
        new("CombSci", "Combined Science", "RA1E"),
        new("EngLang", "English Language", "FK2B"),
        new("EngLit", "English Literature", "FC4"),
        new("Maths", "Mathematics", "RB1"),
        new("Physics", "Physics", "RC1"),
    ];

    public static IReadOnlyList<MeasureSet> MeasureSets() => [Headline(), SubjectAchievements(), SubjectGrades(), Unmodelled()];

    /// <summary>Attainment 8, Progress 8 and English and maths grade 4+/5+.</summary>
    public static MeasureSet Headline()
    {
        var schoolsCurrent = PerformanceTables("202425_performance_tables_schools_final", Current);
        var schoolsPrevious = PerformanceTables("202324_performance_tables_schools_final", Previous);

        // Compare School Performance download: one column per breakdown, so fields are set per breakdown below.
        var schoolsPrevious2 = Source.Cscp("2022-2023_england_ks4final").KeyedBy("URN");

        var englandCurrent = Characteristics("geographic_level", Current);
        var englandPrevious = Characteristics("geographic_level", Previous);
        var englandPrevious2 = Characteristics("geographic_level", Previous2);

        return NewSet("Performance")
            .Source(Scope.England, Period.Current, englandCurrent)
            .Source(Scope.England, Period.Previous, englandPrevious)
            .Source(Scope.England, Period.Previous2, englandPrevious2)
            .Source(Scope.Establishment, Period.Current, schoolsCurrent)
            .Source(Scope.Establishment, Period.Previous, schoolsPrevious)
            .Source(Scope.Establishment, Period.Previous2, schoolsPrevious2)
            .Source(Scope.LA, Period.Current, Characteristics("old_la_code", Current))
            .Source(Scope.LA, Period.Previous, Characteristics("old_la_code", Previous))
            .Source(Scope.LA, Period.Previous2, Characteristics("old_la_code", Previous2))
            .Breakdowns(
                Breakdowns.Total, Breakdowns.Boys, Breakdowns.Girls, Breakdowns.Disadvantaged,
                Breakdowns.NotDisadvantaged, Breakdowns.Eal, Breakdowns.Mobile, Breakdowns.NonMobile)
            .Metric(new Metric("Attainment8", "attainment8_average")
                .Field(schoolsPrevious, "avg_att8")
                .Field(schoolsPrevious2, Breakdowns.Total, "ATT8SCR")
                .Field(schoolsPrevious2, Breakdowns.Boys, "ATT8SCR_BOYS")
                .Field(schoolsPrevious2, Breakdowns.Girls, "ATT8SCR_GIRLS")
                .Field(schoolsPrevious2, Breakdowns.Disadvantaged, "ATT8SCR_FSM6CLA1A")
                .Field(schoolsPrevious2, Breakdowns.NotDisadvantaged, "ATT8SCR_NFSM6CLA1A")
                .Field(schoolsPrevious2, Breakdowns.Eal, "ATT8SCR_EAL")
                .Field(schoolsPrevious2, Breakdowns.NonMobile, "ATT8SCR_NMOB"))
            .Metric(new Metric("EngMaths49", "engmath_94_total")
                .Field(schoolsPrevious, "t_l2basics_94")
                .Skip(Scope.Establishment, Period.Previous2))
            .Metric(new Metric("EngMaths49", "engmath_94_percent")
                .Percentage()
                .Field(schoolsPrevious, "pt_l2basics_94")
                .Field(schoolsPrevious2, Breakdowns.Total, "PTL2BASICS_94")
                .Field(schoolsPrevious2, Breakdowns.Boys, "PBL2BASICS_94")
                .Field(schoolsPrevious2, Breakdowns.Girls, "PGL2BASICS_94")
                .Field(schoolsPrevious2, Breakdowns.Disadvantaged, "PTFSM6CLA1ABASICS_94")
                .Field(schoolsPrevious2, Breakdowns.Eal, "PTL2BASICSEAL_94"))
            .Metric(new Metric("EngMaths59", "engmath_95_total")
                .Field(schoolsPrevious, "t_l2basics_95")
                .Skip(Scope.Establishment, Period.Previous2)
                // DATA ISSUE: England "Mobile" reads the grade 4+ column for this grade 5+ measure.
                .Field(englandCurrent, Breakdowns.Mobile, "engmath_94_total")
                .Field(englandPrevious, Breakdowns.Mobile, "engmath_94_total")
                .Field(englandPrevious2, Breakdowns.Mobile, "engmath_94_total"))
            .Metric(new Metric("EngMaths59", "engmath_95_percent")
                .Percentage()
                .Field(schoolsPrevious, "pt_l2basics_95")
                .Field(schoolsPrevious2, Breakdowns.Total, "PTL2BASICS_95")
                .Field(schoolsPrevious2, Breakdowns.Boys, "PBL2BASICS_95")
                .Field(schoolsPrevious2, Breakdowns.Girls, "PGL2BASICS_95")
                .Field(schoolsPrevious2, Breakdowns.Disadvantaged, "PTFSM6CLA1ABASICS_95")
                .Field(schoolsPrevious2, Breakdowns.Eal, "PTL2BASICSEAL_95")
                // DATA ISSUE: England "Mobile" reads the grade 4+ column for this grade 5+ measure.
                .Field(englandCurrent, Breakdowns.Mobile, "engmath_94_percent")
                .Field(englandPrevious, Breakdowns.Mobile, "engmath_94_percent")
                .Field(englandPrevious2, Breakdowns.Mobile, "engmath_94_percent"))
            .Metric(new Metric("Prog8", "progress8_average")
                .For(Breakdowns.Total)
                .In(Scope.Establishment)
                .Field(schoolsPrevious, "avg_p8score")
                .Field(schoolsPrevious2, Breakdowns.Total, "P8MEA"));
    }

    /// <summary>Pupils achieving grade 9–4, 9–5 and 9–7 in each subject, as a number and a percentage.</summary>
    public static MeasureSet SubjectAchievements()
    {
        var schoolsCurrent = SubjectSchools("custom_202425_subject_school_pupil_entriesachievements_revised", Current);
        var schoolsPrevious = SubjectSchools("custom_202324_subject_school_pupil_entriesachievements_final", Previous);
        var schoolsPrevious2 = SubjectSchools("custom_202223_subject_school_pupil_entriesachievements_final", Previous2);
        (string Code, string Grade)[] bands = [("49", "9 to 4"), ("59", "9 to 5"), ("79", "9 to 7")];

        var set = NewSet("SubjectEntries")
            .Source(Scope.England, Period.Current, SubjectLocalAuthorities("geographic_level", Current))
            .Source(Scope.England, Period.Previous, SubjectLocalAuthorities("geographic_level", Previous))
            .Source(Scope.England, Period.Previous2, SubjectLocalAuthorities("geographic_level", Previous2))
            .Source(Scope.Establishment, Period.Current, schoolsCurrent)
            .Source(Scope.Establishment, Period.Previous, schoolsPrevious)
            .Source(Scope.Establishment, Period.Previous2, schoolsPrevious2)
            .Source(Scope.LA, Period.Current, SubjectLocalAuthorities("old_la_code", Current))
            .Source(Scope.LA, Period.Previous, SubjectLocalAuthorities("old_la_code", Previous))
            .Source(Scope.LA, Period.Previous2, SubjectLocalAuthorities("old_la_code", Previous2))
            .Breakdowns(Breakdowns.Total, Breakdowns.Sum, Breakdowns.Boys, Breakdowns.Girls);

        foreach (var percentage in new[] { false, true })
        {
            foreach (var subject in Subjects)
            {
                foreach (var band in bands)
                {
                    var metric = new Metric($"{subject.Code}{band.Code}", percentage ? "percentage_achieving" : "number_achieving")
                        .Where("subject", subject.Name);

                    // DATA ISSUE: the 2023-24 school file is filtered to grades 7, 8 and 9 for these subjects, which don't
                    // exist in that file (it has a "9 to 7" band), so these values are always blank.
                    if (band.Code == "79" && subject.Code is "EngLang" or "EngLit" or "Maths")
                    {
                        metric.Where(schoolsPrevious, "grade", "7", "8", "9");
                        foreach (var source in set.Sources.Where(s => s != schoolsPrevious))
                            metric.Where(source, "grade", band.Grade);
                    }
                    else
                    {
                        metric.Where("grade", band.Grade);
                    }

                    set.Metric(percentage ? metric.Percentage() : metric);
                }
            }
        }

        return set;
    }

    /// <summary>Entries at each grade per subject (Combined Science uses double grades, e.g. 98).</summary>
    public static MeasureSet SubjectGrades()
    {
        var current = SubjectExamEntries("202425_subject_school_all_exam_entriesgrades_final", Current);
        var previous = SubjectExamEntries("202324_subject_school_all_exam_entriesgrades_final", Previous);

        // Older underlying data identifies subjects by discount code.
        var previous2 = Source.Cscp("england_ks4underlying_entriesandgrades_2")
            .KeyedBy("Unique Reference Number (URN)")
            .Provides(Breakdowns.Sum);

        string[] singleGrades = ["4", "5", "6", "7", "8", "9"];
        string[] doubleGrades = ["43", "44", "54", "55", "65", "66", "76", "77", "87", "88", "98", "99"];

        var set = NewSet("SubjectEntries")
            .Source(Scope.Establishment, Period.Current, current)
            .Source(Scope.Establishment, Period.Previous, previous)
            .Source(Scope.Establishment, Period.Previous2, previous2)
            .Breakdowns(Breakdowns.Sum);

        foreach (var subject in Subjects)
        {
            foreach (var grade in subject.Code == "CombSci" ? doubleGrades : singleGrades)
            {
                set.Metric(new Metric($"{subject.Code}{grade}", "number_achieving")
                    .Where(current, "subject", subject.Name).Where(current, "grade", grade)
                    .Where(previous, "subject", subject.Name).Where(previous, "grade", grade)
                    .Where(previous2, "Discount Code", subject.DiscountCode).Where(previous2, "GRADE", grade)
                    .Field(previous2, "Number of entries at grade"));
            }
        }

        return set;
    }

    /// <summary>Rows kept as-is from datamap.csv because they don't fit the catalogue model.</summary>
    public static MeasureSet Unmodelled() =>
        new MeasureSet(Type, "Unmodelled")
            // DATA ISSUE: named Previous2 but reads 2024-25, filtered only on sex = Total so MAX runs across every
            // breakdown topic. datamap.csv also had Previous and Previous2 rows under this name; the generator used this one.
            .Row(new DataMapRow
            {
                Range = "LA",
                Ref = "24_KS4_P8_TOT_LA",
                PropertyName = "Prog8_Avg_LA_Previous2_Num",
                PropertyDescription = "Progress 8",
                Source = "EES",
                Type = Type,
                Subtype = "Performance",
                Year = "2024-2025",
                YearDesc = "Current",
                FileName = "202425_all_state_funded_pupils_characteristics_and_geography_breakdowns_revised",
                Field = "progress8_average",
                DataType = "double",
                RecordFilterBy = "old_la_code",
                Filter = "sex",
                FilterValue = "Total",
                Filter3 = "time_period",
                Filter3Value = "202425",
            });

    private static MeasureSet NewSet(string subtype) => new MeasureSet(Type, subtype).Years(CurrentYear);

    /// <summary>EES school performance tables: one row per school and breakdown.</summary>
    private static Source PerformanceTables(string file, AcademicYear year) =>
        Source.Ees(file)
            .KeyedBy("school_urn")
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total, ("breakdown", "Total"))
            .Provides(Breakdowns.Boys, ("breakdown", "Boys"))
            .Provides(Breakdowns.Girls, ("breakdown", "Girls"))
            .Provides(Breakdowns.Disadvantaged, ("breakdown", "Disadvantaged"))
            .Provides(Breakdowns.NotDisadvantaged, ("breakdown", "Not known to be disadvantaged"))
            .Provides(Breakdowns.Eal, ("breakdown", "Other than English"))
            .Provides(Breakdowns.NonMobile, ("breakdown", "Non mobile"));

    /// <summary>EES pupil characteristics and geography breakdowns: every year in one file, LA and England rows.</summary>
    private static Source Characteristics(string key, AcademicYear year) =>
        Source.Ees("202425_all_state_funded_pupils_characteristics_and_geography_breakdowns_revised")
            .KeyedBy(key)
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total, ("breakdown_topic", "Total"))
            .Provides(Breakdowns.Boys, ("breakdown_topic", "Sex"), ("sex", "Boys"))
            .Provides(Breakdowns.Girls, ("breakdown_topic", "Sex"), ("sex", "Girls"))
            .Provides(Breakdowns.Disadvantaged, ("breakdown_topic", "Disadvantage status"), ("disadvantage_status", "Disadvantaged"))
            .Provides(Breakdowns.NotDisadvantaged, ("breakdown_topic", "Disadvantage status"), ("disadvantage_status", "Not known to be disadvantaged"))
            .Provides(Breakdowns.Eal, ("breakdown_topic", "First language"), ("first_language", "Known or believed to be other than English"))
            .Provides(Breakdowns.Mobile, ("breakdown_topic", "Mobility"), ("mobility", "Mobile"));

    /// <summary>Custom subject achievement extract for schools: whole-school totals only.</summary>
    private static Source SubjectSchools(string file, AcademicYear year) =>
        Source.Ees(file)
            .KeyedBy("school_urn")
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Sum);

    /// <summary>Custom subject achievement extract for LAs and England: every year in one file, by sex.</summary>
    private static Source SubjectLocalAuthorities(string key, AcademicYear year) =>
        Source.Ees("custom_202325_subject_local_authority_pupil_entriesachievements")
            .KeyedBy(key)
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total, ("sex", "Total"))
            .Provides(Breakdowns.Boys, ("sex", "Boys"))
            .Provides(Breakdowns.Girls, ("sex", "Girls"));

    /// <summary>EES all exam entries and grades by subject for schools.</summary>
    private static Source SubjectExamEntries(string file, AcademicYear year) =>
        Source.Ees(file)
            .KeyedBy("school_urn")
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Sum);

    private sealed record Subject(string Code, string Name, string DiscountCode);
}
