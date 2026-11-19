using Dfe.Analytics;
using Dfe.Analytics.AspNetCore;
using SAPSec.Core.CustomEvents;
using SAPSec.Web.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DfeAnalyticsExtensions
{
    private static readonly bool isLocalDevelopment = Environment.GetEnvironmentVariable("IS_LOCAL_DEVELOPMENT") == "true";

    public static void AddDfeAnalyticsDependencies(this IServiceCollection services, IWebHostEnvironment environment, bool bypassTestAuthentication)
    {
        if (isLocalDevelopment || bypassTestAuthentication)
        {
            services.AddScoped<ICustomEventService, NoOpCustomEventService>();
        }
        else
        {
            services.AddScoped<ICustomEventService, CustomEventService>();
        }

        if (ShouldUseAnalytics(environment, bypassTestAuthentication))
        {
            services.AddDfeAnalytics().AddAspNetCoreIntegration(options =>
            {
                options.RequestFilter = ctx =>
                    ctx.Request.Path != "/healthcheck";
            });
        }
    }

    public static void UseAnalytics(this WebApplication app, IWebHostEnvironment environment, bool bypassTestAuthentication)
    {
        if (ShouldUseAnalytics(environment, bypassTestAuthentication))
        {
            app.UseDfeAnalytics();
        }
    }

    private static bool ShouldUseAnalytics(IWebHostEnvironment environment, bool bypassTestAuthentication) =>
        !isLocalDevelopment && !bypassTestAuthentication &&
        environment.EnvironmentName is not ("UITests" or "IntegrationTests" or "EndToEndTests" or "AccessibilityTests" or "LoadTest" or "Test");
}
