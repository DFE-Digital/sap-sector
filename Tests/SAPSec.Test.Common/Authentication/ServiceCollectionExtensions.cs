using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Core.Authentication;
using SAPSec.Web.Authorization;

namespace SAPSec.Test.Common.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestDsiDependencies(this IServiceCollection services, bool enforceUrnAccess = false)
    {
        services.RemoveAll<IUserService>();
        services.RemoveAll<IDsiClient>();
        services.AddScoped<IUserService, MockDsiUserService>();
        services.AddScoped<IDsiClient, MockDsiApiService>();

        if (!enforceUrnAccess)
        {
            var real = services.Where(d => d.ServiceType == typeof(IAuthorizationHandler)
                                           && d.ImplementationType == typeof(DsiAuthorizationHandler)).ToList();
            foreach (var descriptor in real)
            {
                services.Remove(descriptor);
            }

            services.AddScoped<IAuthorizationHandler, AllowAllUrnsDsiAuthorizationHandler>();
        }

        return services;
    }
}
