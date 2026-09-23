namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>School workforce: pupil to qualified teacher ratio and pupil numbers, current year only.</summary>
/// <remarks>To roll to a new year, bump <see cref="DataYears"/> and point the source at the new file.</remarks>
public static class Workforce
{
    public const string Type = "Workforce";

    private const int CurrentYear = DataYears.Workforce;

    public static IReadOnlyList<IDataMapDefinition> Definitions() =>
    [
        new MeasureSet(Type, "Workforce")
            .Year(Period.Current, new AcademicYear(CurrentYear))
            .Source(Scope.Establishment, Period.Current, Source.Ees("workforce_ptrs_2010_2025_sch")
                .KeyedBy("school_urn")
                .Where("time_period", new AcademicYear(CurrentYear).Code)
                .Provides(Breakdowns.Total))
            .Metric(new Metric("Workforce_PupTeaRatio", "pupil_to_qual_teacher_ratio").Named("{metric}_{scope}_{period}_{unit}"))
            .Metric(new Metric("Workforce_TotPupils", "pupils_fte").Named("{metric}_{scope}_{period}_{unit}")),
    ];
}
