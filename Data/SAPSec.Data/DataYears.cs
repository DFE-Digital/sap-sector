namespace SAPSec.Data;

/// <summary>
/// The latest academic year loaded for each dataset, as the start year (2024 = 2024 to 2025).
/// Each dataset holds this year and the two before it.
/// </summary>
/// <remarks>
/// Shared by the data pipeline (which files and time periods to load) and the website (year labels), so both
/// always agree. To roll a dataset to a new year, follow docs/operational/003-new-data-year.md.
/// </remarks>
public static class DataYears
{
    public const int Ks4Performance = 2024;

    /// <summary>Destinations are published a year later and labelled by the cohort's KS4 year.</summary>
    public const int Ks4Destinations = 2022;

    public const int Ks2Performance = 2024;

    public const int PupilAbsence = 2024;

    public const int Workforce = 2024;
}
