namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// KS4 destination measures: pupils in sustained education, employment or apprenticeships after KS4.
/// </summary>
/// <remarks>
/// Years are the cohort's KS4 year (the file's time_period), which is what the website labels them as.
/// To roll to a new year, bump <see cref="CurrentYear"/> and point <see cref="File"/> at the new release.
/// </remarks>
public static class Ks4Destinations
{
    public const string Type = "KS4_Destinations";

    private const int CurrentYear = 2022;
    private const string File = "ees_ks4_202223_api";
    private static readonly AcademicYear Current = new(CurrentYear);
    private static readonly AcademicYear Previous = new(CurrentYear - 1);
    private static readonly AcademicYear Previous2 = new(CurrentYear - 2);

    private static readonly (string Name, string Group, string Description)[] DestinationTypes =
    [
        ("AllDest", "Overall destination", "Sustained education, employment & apprenticeships"),
        ("Education", "Headline destinations", "Sustained education destination"),
        ("Employment", "Headline destinations", "Sustained employment destination"),
        ("Apprentice", "Headline destinations", "Sustained apprenticeships"),
    ];

    public static IReadOnlyList<MeasureSet> MeasureSets() => [Destinations()];

    public static MeasureSet Destinations()
    {
        var set = new MeasureSet(Type, "Destinations")
            .Years(CurrentYear)
            .Source(Scope.England, Period.Current, DestinationsFile("geographic_level", Current, "Total state-funded mainstream and special", includeEal: true))
            .Source(Scope.England, Period.Previous, DestinationsFile("geographic_level", Previous, "Total state-funded mainstream and special", includeEal: true))
            .Source(Scope.England, Period.Previous2, DestinationsFile("geographic_level", Previous2, "Total state-funded mainstream and special", includeEal: true))
            .Source(Scope.Establishment, Period.Current, DestinationsFile("school_urn", Current))
            .Source(Scope.Establishment, Period.Previous, DestinationsFile("school_urn", Previous))
            .Source(Scope.Establishment, Period.Previous2, DestinationsFile("school_urn", Previous2))
            .Source(Scope.LA, Period.Current, DestinationsFile("old_la_code", Current, "State-funded mainstream and special"))
            .Source(Scope.LA, Period.Previous, DestinationsFile("old_la_code", Previous, "State-funded mainstream and special"))
            .Source(Scope.LA, Period.Previous2, DestinationsFile("old_la_code", Previous2, "State-funded mainstream and special"))
            .Breakdowns(
                Breakdowns.Total, Breakdowns.Boys, Breakdowns.Girls,
                Breakdowns.Disadvantaged, Breakdowns.NotDisadvantaged, Breakdowns.Eal);

        foreach (var (name, group, description) in DestinationTypes)
        {
            set.Metric(new Metric(name, "pupil_count")
                    .Where("destination_group", group)
                    .Where("destination_description", description))
                .Metric(new Metric(name, "pupil_percent")
                    .Percentage()
                    .Where("destination_group", group)
                    .Where("destination_description", description));
        }

        return set;
    }

    /// <summary>
    /// EES KS4 destination measures: one file with every year and level. Breakdowns select a single pupil group,
    /// leaving the other characteristics at Total.
    /// </summary>
    private static Source DestinationsFile(string key, AcademicYear year, string? establishmentTypeGroup = null, bool includeEal = false)
    {
        var source = Source.Ees(File).KeyedBy(key);

        if (establishmentTypeGroup is not null)
            source.Where("establishment_type_group", establishmentTypeGroup);

        source
            .Where("ethnicity_major", "Total")
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total, ("sex", "Total"), ("disadvantage_status", "Total"), ("breakdown_topic", "Total"), ("breakdown", "Total"))
            .Provides(Breakdowns.Boys, ("sex", "Male"), ("disadvantage_status", "Total"), ("breakdown_topic", "Total"), ("breakdown", "Total"))
            .Provides(Breakdowns.Girls, ("sex", "Female"), ("disadvantage_status", "Total"), ("breakdown_topic", "Total"), ("breakdown", "Total"))
            .Provides(Breakdowns.Disadvantaged, ("sex", "Total"), ("disadvantage_status", "Disadvantaged"), ("breakdown_topic", "Total"), ("breakdown", "Total"))
            .Provides(Breakdowns.NotDisadvantaged, ("sex", "Total"), ("disadvantage_status", "Not known to be disadvantaged"), ("breakdown_topic", "Total"), ("breakdown", "Total"));

        if (includeEal)
            source.Provides(Breakdowns.Eal, ("sex", "Total"), ("disadvantage_status", "Total"), ("breakdown_topic", "First language"), ("breakdown", "Known or believed to be other than English"));

        return source;
    }
}
