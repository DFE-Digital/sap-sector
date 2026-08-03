using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SAPSec.Web.Configuration;

public static class SentryConfiguration
{
    public static SentrySettings GetSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection(SentrySettings.SectionName).Get<SentrySettings>() ?? new SentrySettings();

        if (!string.IsNullOrWhiteSpace(settings.Dsn))
        {
            return settings;
        }

        var dsn = configuration["SENTRY_DSN"];
        if (string.IsNullOrWhiteSpace(dsn))
        {
            return settings;
        }

        return new SentrySettings
        {
            Enabled = settings.Enabled,
            Dsn = dsn,
            Environment = settings.Environment,
            Debug = settings.Debug,
            MinimumBreadcrumbLevel = settings.MinimumBreadcrumbLevel,
            MinimumEventLevel = settings.MinimumEventLevel
        };
    }

    public static bool IsEnabled(SentrySettings settings)
        => settings.Enabled && !string.IsNullOrWhiteSpace(settings.Dsn);

    public static string GetEnvironmentName(IConfiguration configuration, string? environmentName)
    {
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            return Environments.Production.ToLowerInvariant();
        }

        return environmentName.Trim().ToLowerInvariant();
    }

    public static LogLevel GetMinimumBreadcrumbLevel(SentrySettings settings)
        => ParseLogLevel(settings.MinimumBreadcrumbLevel, LogLevel.Information);

    public static LogLevel GetMinimumEventLevel(SentrySettings settings)
        => ParseLogLevel(settings.MinimumEventLevel, LogLevel.Error);

    private static LogLevel ParseLogLevel(string? value, LogLevel fallback)
        => Enum.TryParse<LogLevel>(value, ignoreCase: true, out var level)
            ? level
            : fallback;
}
