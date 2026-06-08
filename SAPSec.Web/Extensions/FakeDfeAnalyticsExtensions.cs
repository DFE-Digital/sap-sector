using Dfe.Analytics.Events;
using SAPSec.Web.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class FakeDfeAnalyticsExtensions
{
    public static void AddFakeDfeAnalytics(this IServiceCollection services)
    {
        services.AddTransient<IEventSender, FakeEventSenderService>();
    }
}