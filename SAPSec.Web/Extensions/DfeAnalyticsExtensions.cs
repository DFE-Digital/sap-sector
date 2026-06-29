using Dfe.Analytics;
using Dfe.Analytics.AspNetCore;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DfeAnalyticsExtensions
{
    private static readonly bool isLocalDevelopment = Environment.GetEnvironmentVariable("IS_LOCAL_DEVELOPMENT") == "true";

    public static void AddDfeAnalyticsDependencies(this IServiceCollection services, IWebHostEnvironment environment)
    {
        if (isLocalDevelopment)
        {
            services.AddScoped<ICustomEventService, NoOpCustomEventService>();
        }
        else
        {
            services.AddScoped<ICustomEventService, CustomEventService>();
        }

        if (environment.EnvironmentName is not ("UITests" or "IntegrationTests" or "EndToEndTests" or "AccessibilityTests") && !isLocalDevelopment)
        {
            services.AddDfeAnalytics().AddAspNetCoreIntegration(options =>
            {
                options.RequestFilter = ctx =>
                    ctx.Request.Path != "/healthcheck";
            });
        }
    }

    public static void UseAnalytics(this WebApplication app, IWebHostEnvironment environment)
    {
        if (environment.EnvironmentName is not ("UITests" or "IntegrationTests" or "EndToEndTests" or "AccessibilityTests") && !isLocalDevelopment)
        {
            app.UseDfeAnalytics();
        }
    }
}