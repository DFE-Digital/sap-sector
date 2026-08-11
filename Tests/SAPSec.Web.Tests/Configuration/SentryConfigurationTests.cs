using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAPSec.Web.Configuration;

namespace SAPSec.Web.Tests.Configuration;

public class SentryConfigurationTests
{
    [Fact]
    public void GetSettings_UsesEnvironmentVariableDsn_WhenSectionDsnMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Sentry:Enabled"] = "true",
            ["SENTRY_DSN"] = "https://example.ingest.sentry.io/123"
        });

        var result = SentryConfiguration.GetSettings(configuration);

        result.Dsn.Should().Be("https://example.ingest.sentry.io/123");
        SentryConfiguration.IsEnabled(result).Should().BeTrue();
    }

    [Theory]
    [InlineData("Production", "production")]
    [InlineData("Test", "test")]
    [InlineData("Development", "development")]
    public void GetEnvironmentName_UsesAspNetCoreEnvironmentName(string environmentName, string expected)
    {
        var configuration = BuildConfiguration();

        var result = SentryConfiguration.GetEnvironmentName(configuration, environmentName);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetEnvironmentName_PrefersConfiguredSentryEnvironment()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Sentry:Environment"] = "pre-production"
        });

        var result = SentryConfiguration.GetEnvironmentName(configuration, "Production");

        result.Should().Be("pre-production");
    }

    [Fact]
    public void GetEnvironmentName_FallsBackToProduction_WhenNoEnvironmentConfigured()
    {
        var configuration = BuildConfiguration();

        var result = SentryConfiguration.GetEnvironmentName(configuration, null);

        result.Should().Be("production");
    }

    [Fact]
    public void GetMinimumLevels_FallBackToExpectedDefaults()
    {
        var settings = new SentrySettings();

        SentryConfiguration.GetMinimumBreadcrumbLevel(settings).Should().Be(LogLevel.Information);
        SentryConfiguration.GetMinimumEventLevel(settings).Should().Be(LogLevel.Error);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? values = null)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? new Dictionary<string, string?>())
            .Build();
}
