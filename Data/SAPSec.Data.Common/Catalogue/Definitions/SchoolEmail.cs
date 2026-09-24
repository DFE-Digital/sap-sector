namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>School contact email addresses (GIAS extract). Point <see cref="File"/> at a new extract to refresh.</summary>
public static class SchoolEmail
{
    public const string Type = "Email";

    private const string File = "school-email-addresses-24-08-2026";

    public static IReadOnlyList<IDataMapDefinition> Definitions() =>
    [
        new ColumnSet(Type, Scope.Establishment.ToString(), Source.Gias(File).KeyedBy("URN"))
            .Column("URN", "URN")
            .Column("EstablishmentNumber", "EstablishmentNumber")
            .Column("EstablishmentName", "EstablishmentName")
            .Column("TypeOfEstablishmentName", "TypeOfEstablishment (name)")
            .Column("EstablishmentTypeGroupName", "EstablishmentTypeGroup (name)")
            .Column("EstablishmentStatusName", "EstablishmentStatus (name)")
            .Column("CloseDate", "CloseDate")
            .Column("PhaseOfEducationName", "PhaseOfEducation (name)")
            .Column("MainEmail", "MainEmail"),
    ];
}
