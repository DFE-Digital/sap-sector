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

        // TEMPORARY: "Test" added here alongside the auth bypass in Program.cs -
        // real DfE Analytics (Dfe.Analytics.AspNetCore.DfeAnalyticsMiddleware)
        // throws "BigQueryClient has not been configured" on every request,
        // including /healthcheck, when valid GCP credentials aren't available.
        // Also avoids polluting real test analytics with synthetic load-test
        // traffic. Revert alongside the Program.cs auth bypass before merge.
        if (environment.EnvironmentName is not ("UITests" or "IntegrationTests" or "EndToEndTests" or "AccessibilityTests" or "LoadTest" or "Test") && !isLocalDevelopment)
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
        // TEMPORARY: "Test" added here alongside the auth bypass in Program.cs -
        // real DfE Analytics (Dfe.Analytics.AspNetCore.DfeAnalyticsMiddleware)
        // throws "BigQueryClient has not been configured" on every request,
        // including /healthcheck, when valid GCP credentials aren't available.
        // Also avoids polluting real test analytics with synthetic load-test
        // traffic. Revert alongside the Program.cs auth bypass before merge.
        if (environment.EnvironmentName is not ("UITests" or "IntegrationTests" or "EndToEndTests" or "AccessibilityTests" or "LoadTest" or "Test") && !isLocalDevelopment)
        {
            app.UseDfeAnalytics();
        }
    }
}