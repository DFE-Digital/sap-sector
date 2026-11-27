using Dfe.Analytics;
using Dfe.Analytics.AspNetCore;
using SAPSec.Core.CustomEvents;
using SAPSec.Web.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Analytics;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddAnalytics(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
        services.Configure<AnalyticsSettings>(configuration.GetSection(AnalyticsSettings.SectionName));
        var settings = configuration.GetSection(AnalyticsSettings.SectionName).Get<AnalyticsSettings>();
        if (settings?.EnableAnalytics is true)
        {
            services.AddScoped<ICustomEventService, CustomEventService>();
            services.Configure<CustomEventLocations>(configuration.GetSection(CustomEventLocations.SectionName));
            services.AddDfeAnalytics().AddAspNetCoreIntegration(options =>
            {
                options.RequestFilter = ctx =>
                    ctx.Request.Path != "/healthcheck";
            });

            services.PostConfigure<AnalyticsSettings>(options =>
            {
                var environmentName = configuration["ENVIRONMENT_NAME"] ?? environment.EnvironmentName;
                var analyticsEnvironment = IsProductionEnvironment(environmentName) ? "production" : "test";

                if (options.GoogleTagManagerIds?.TryGetValue(analyticsEnvironment, out var googleTagManagerId) == true)
                {
                    options.GoogleTagManagerId = googleTagManagerId;
                }

                if (options.GoogleTagManagerAdditionals?.TryGetValue(analyticsEnvironment, out var googleTagManagerAdditional) == true)
                {
                    options.GoogleTagManagerAdditional = googleTagManagerAdditional;
                }

                if (options.ClarityIds?.TryGetValue(analyticsEnvironment, out var clarityId) == true)
                {
                    options.ClarityId = clarityId;
                }
            });
        }
        else
        {
            services.AddScoped<ICustomEventService, NoOpCustomEventService>();
        }
    }

    public static void UseAnalytics(this WebApplication app, IConfiguration configuration)
    {
        var settings = configuration.GetSection(AnalyticsSettings.SectionName).Get<AnalyticsSettings>();
        if (settings?.EnableAnalytics is true)
        {
            app.UseDfeAnalytics();
        }
    }

    private static bool IsProductionEnvironment(string? environmentName)
    {
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            return false;
        }

        return environmentName.Equals("production", StringComparison.OrdinalIgnoreCase)
               || environmentName.Equals("prod", StringComparison.OrdinalIgnoreCase)
               || environmentName.Equals("pd", StringComparison.OrdinalIgnoreCase);
    }
}
