namespace SAPSec.Web.Configuration;

public sealed class SentrySettings
{
    public const string SectionName = "Sentry";

    public bool Enabled { get; init; }

    public string? Dsn { get; init; }

    public string? Environment { get; init; }

    public bool Debug { get; init; }

    public string MinimumBreadcrumbLevel { get; init; } = "Information";

    public string MinimumEventLevel { get; init; } = "Error";
}
