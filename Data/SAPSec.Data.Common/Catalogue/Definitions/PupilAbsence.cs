namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// Pupil absence: overall absence rate and persistent absence rate, for schools and, by phase, for LAs and England.
/// </summary>
/// <remarks>
/// Totals and pupil-group breakdowns come from different files in some years, so each file has its own measure set.
/// To roll to a new year, bump <see cref="CurrentYear"/> and point each source at the new files.
/// Property names: Abs_{Tot|Persistent}[_{breakdown}][_{Primary|Secondary}]_{scope}_{period}_Pct,
/// where the breakdown is left out for all pupils and the phase only appears for LAs and England.
/// </remarks>
public static class PupilAbsence
{
    public const string Type = "PupilAbsence";

    private const int CurrentYear = 2024;
    private static readonly AcademicYear Current = new(CurrentYear);
    private static readonly AcademicYear Previous = new(CurrentYear - 1);
    private static readonly AcademicYear Previous2 = new(CurrentYear - 2);

    private static readonly Breakdown[] PupilGroups =
    [
        Breakdowns.Boys, Breakdowns.Girls, Breakdowns.Disadvantaged,
        Breakdowns.NotDisadvantaged, Breakdowns.Eal, Breakdowns.FirstLanguageEnglish,
    ];

    // Overall absence ("Tot") and persistent absence: the field in the EES files, and the abbreviation used in the
    // school-level wide files (boy_oa_percent, boy_pa_percent).
    private static readonly (string Name, string Field, string SchoolSuffix)[] Measures =
    [
        ("Tot", "sess_overall_percent", "oa"),
        ("Persistent", "enrolments_pa_10_exact_percent", "pa"),
    ];

    private static readonly (string Name, string EducationPhase)[] Phases =
    [
        ("Primary", "State-funded primary"),
        ("Secondary", "State-funded secondary"),
    ];

    public static IReadOnlyList<MeasureSet> MeasureSets() =>
        [LocalAuthorityAndEnglandByPupilGroup(), LocalAuthorityAndEnglandTotals(), .. Measures.Select(SchoolsByPupilGroup), SchoolTotals()];

    /// <summary>LA and England, by phase: every pupil group, plus the all-pupils total for the current year.</summary>
    public static MeasureSet LocalAuthorityAndEnglandByPupilGroup()
    {
        var set = new MeasureSet(Type, "Absence")
            .Years(CurrentYear)
            .Source(Scope.England, Period.Current, Characteristics("geographic_level", Current, includeTotal: true))
            .Source(Scope.England, Period.Previous, Characteristics("geographic_level", Previous))
            .Source(Scope.England, Period.Previous2, Characteristics("geographic_level", Previous2))
            .Source(Scope.LA, Period.Current, Characteristics("old_la_code", Current, includeTotal: true))
            .Source(Scope.LA, Period.Previous, Characteristics("old_la_code", Previous))
            .Source(Scope.LA, Period.Previous2, Characteristics("old_la_code", Previous2))
            .Breakdowns([Breakdowns.Total, .. PupilGroups]);

        foreach (var measure in Measures)
            foreach (var phase in Phases)
                set.Metric(PhaseMetric(measure.Name, measure.Field, phase));

        return set;
    }

    /// <summary>LA and England all-pupils totals, by phase, for the earlier years.</summary>
    public static MeasureSet LocalAuthorityAndEnglandTotals()
    {
        var set = new MeasureSet(Type, "Absence")
            .Year(Period.Previous, Previous)
            .Year(Period.Previous2, Previous2)
            .Source(Scope.England, Period.Previous, NationalAndLocalAuthority("geographic_level", Previous))
            .Source(Scope.England, Period.Previous2, NationalAndLocalAuthority("geographic_level", Previous2))
            .Source(Scope.LA, Period.Previous, NationalAndLocalAuthority("old_la_code", Previous))
            .Source(Scope.LA, Period.Previous2, NationalAndLocalAuthority("old_la_code", Previous2))
            .Breakdowns(Breakdowns.Total);

        foreach (var measure in Measures)
            foreach (var phase in Phases)
                set.Metric(PhaseMetric(measure.Name, measure.Field, phase));

        return set;
    }

    /// <summary>Schools, by pupil group, from the wide school files (one column per group), plus the current-year total.</summary>
    public static MeasureSet SchoolsByPupilGroup((string Name, string Field, string SchoolSuffix) measure)
    {
        var current = SchoolWide(measure.SchoolSuffix, Current);
        var previous = SchoolWide(measure.SchoolSuffix, Previous);
        var previous2 = SchoolWide(measure.SchoolSuffix, Previous2);

        var metric = new Metric($"Abs_{measure.Name}", "")
            .Percentage()
            .Named(SchoolName($"Abs_{measure.Name}"))
            .Field(current, Breakdowns.Total, $"all_{measure.SchoolSuffix}_percent");

        foreach (var source in new[] { current, previous, previous2 })
            foreach (var group in PupilGroups)
                metric.Field(source, group, $"{WideColumnGroup(group)}_{measure.SchoolSuffix}_percent");

        return new MeasureSet(Type, "Absence")
            .Years(CurrentYear)
            .Source(Scope.Establishment, Period.Current, current)
            .Source(Scope.Establishment, Period.Previous, previous)
            .Source(Scope.Establishment, Period.Previous2, previous2)
            .Breakdowns([Breakdowns.Total, .. PupilGroups])
            .Metric(metric);
    }

    /// <summary>School all-pupils totals for the earlier years.</summary>
    public static MeasureSet SchoolTotals()
    {
        var set = new MeasureSet(Type, "Absence")
            .Year(Period.Previous, Previous)
            .Year(Period.Previous2, Previous2)
            .Source(Scope.Establishment, Period.Previous, SchoolTotalsFile(Previous))
            .Source(Scope.Establishment, Period.Previous2, SchoolTotalsFile(Previous2))
            .Breakdowns(Breakdowns.Total);

        foreach (var measure in Measures)
            set.Metric(new Metric($"Abs_{measure.Name}", measure.Field).Percentage().Named(SchoolName($"Abs_{measure.Name}")));

        return set;
    }

    private static Metric PhaseMetric(string measure, string field, (string Name, string EducationPhase) phase) =>
        new Metric($"Abs_{measure}_{phase.Name}", field)
            .Percentage()
            .Where("education_phase", phase.EducationPhase)
            .Named((breakdown, scope, period) =>
                string.Join('_', Segments($"Abs_{measure}", breakdown, phase.Name, scope, period)));

    private static Func<Breakdown, Scope, Period, string> SchoolName(string measure) =>
        (breakdown, scope, period) => string.Join('_', Segments(measure, breakdown, phase: null, scope, period));

    private static IEnumerable<string> Segments(string measure, Breakdown breakdown, string? phase, Scope scope, Period period)
    {
        yield return measure;
        if (breakdown != Breakdowns.Total)
            yield return breakdown.Code;
        if (phase is not null)
            yield return phase;
        yield return scope.NameCode();
        yield return period.ToString();
        yield return "Pct";
    }

    /// <summary>EES absence by pupil characteristics: every year in one file, LA and England rows.</summary>
    private static Source Characteristics(string key, AcademicYear year, bool includeTotal = false)
    {
        var source = Source.Ees("6_absence_3term_characteristics")
            .KeyedBy(key)
            .Where("time_period", year.Code);

        if (includeTotal)
            source.Provides(Breakdowns.Total, ("breakdown", "Total"));

        return source
            .Provides(Breakdowns.Boys, ("breakdown", "Male"))
            .Provides(Breakdowns.Girls, ("breakdown", "Female"))
            .Provides(Breakdowns.Disadvantaged, ("breakdown", "FSM eligible in last 6 years"))
            .Provides(Breakdowns.NotDisadvantaged, ("breakdown", "FSM not eligible in last 6 years"))
            .Provides(Breakdowns.Eal, ("breakdown", "First language known or believed to be other than English"))
            .Provides(Breakdowns.FirstLanguageEnglish, ("breakdown", "First language known or believed to be English"));
    }

    /// <summary>EES absence for England, regions and LAs: all-pupils figures only.</summary>
    private static Source NationalAndLocalAuthority(string key, AcademicYear year) =>
        Source.Ees("1_absence_3term_nat_reg_la")
            .KeyedBy(key)
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total);

    /// <summary>EES absence for schools: all-pupils figures only.</summary>
    private static Source SchoolTotalsFile(AcademicYear year) =>
        Source.Ees("1a_absence_3term_school")
            .KeyedBy("school_urn")
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total);

    /// <summary>School absence by pupil group, one column per group: oa_percent_… (overall) or pa_percent_… (persistent).</summary>
    private static Source SchoolWide(string suffix, AcademicYear year) =>
        Source.Ees($"{suffix}_percent_3term_sch_{year.Code}_ccss")
            .KeyedBy("urn")
            .Where("year", year.Code);

    private static string WideColumnGroup(Breakdown breakdown) => breakdown.Code switch
    {
        "Boy" => "boy",
        "Grl" => "girl",
        "Dis" => "fsm",
        "NDi" => "nonfsm",
        "EAL" => "othlang",
        "EFL" => "englang",
        _ => throw new ArgumentOutOfRangeException(nameof(breakdown), breakdown.Code, null)
    };
}
