using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Core.Authentication;

namespace SAPSec.Test.Common.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestDsiDependencies(this IServiceCollection services)
    {
        services.RemoveAll<IUserService>();
        services.RemoveAll<IDsiClient>();
        services.AddScoped<IUserService, MockDsiUserService>();
        services.AddScoped<IDsiClient, MockDsiApiService>();

        return services;
    }
}
