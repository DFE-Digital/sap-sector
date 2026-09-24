namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// Similar schools: each school's neighbours (groups) and the values used to match them, for primary and secondary.
/// Read by SAPData.GenerateSimilarSchoolsViews, which copies the columns as they are.
/// </summary>
/// <remarks>To refresh, point <see cref="Release"/> at the new files' date prefix.</remarks>
public static class SimilarSchools
{
    public const string Range = "SimilarSchools";
    public const string PrimaryGroups = "PrimaryGroups";
    public const string SecondaryGroups = "SecondaryGroups";
    public const string PrimaryValues = "PrimaryValues";
    public const string SecondaryValues = "SecondaryValues";

    private const string Release = "2026_07_03";

    public static IReadOnlyList<string> Types { get; } = [PrimaryGroups, SecondaryGroups, PrimaryValues, SecondaryValues];

    public static IReadOnlyList<IDataMapDefinition> Definitions() =>
    [
        Groups(PrimaryGroups, $"{Release}_neighbours_list_primary"),
        Groups(SecondaryGroups, $"{Release}_neighbours_list_secondary"),

        Values(PrimaryValues, $"{Release}_matched_primary_schools_data")
            .Column("Ks1PriorRwmAverage", "ks1_prior_rwm_avg")
            .Column("PPPerc", "percent_fsm_ever")
            .Column("Polar4QuintilePupils", "polar4quintile_pupils")
            .Column("PStability", "p_stability")
            .Column("PercentSchSupport", "percent_sch_support")
            .Column("PercentEAL", "percent_eal")
            .Column("IdaciPupils", "idaci_pupils")
            .Column("PercentageStatementOrEhp", "percent_statement_or_ehp")
            .Column("NumberOfPupils", "nor")
            .Column("ReadMatAverage", "ks2_outcome_read_mat_avg"),

        Values(SecondaryValues, $"{Release}_matched_secondary_schools_data")
            .Column("KS2MRP", "ks2_prior_rm_avg")
            .Column("PPPerc", "percent_fsm_ever")
            .Column("PercentEAL", "percent_eal")
            .Column("Polar4QuintilePupils", "polar4quintile_pupils")
            .Column("PStability", "p_stability")
            .Column("IdaciPupils", "idaci_pupils")
            .Column("PercentSchSupport", "percent_sch_support")
            .Column("NumberOfPupils", "nor")
            .Column("PercentageStatementOrEHP", "percent_statement_or_ehp")
            .Column("Att8Scr", "ks4_outcome_a8"),
    ];

    private static ColumnSet Groups(string type, string file) =>
        new ColumnSet(type, Range, Source.From("Similar Schools", file).KeyedBy("urn"))
            .Column("URN", "urn")
            .Column("NeighbourURN", "neighbour_urn")
            .Column("Dist", "dist")
            .Column("Rank", "rank");

    private static ColumnSet Values(string type, string file) =>
        new ColumnSet(type, Range, Source.From("Similar Schools", file).KeyedBy("urn"))
            .Column("URN", "urn");
}
