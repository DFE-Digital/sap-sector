using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Core.Authentication;
using SAPSec.Web.Authorization;

namespace SAPSec.Test.Common.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestDsiDependencies(this IServiceCollection services)
    {
        services.RemoveAll<IUserService>();
        services.RemoveAll<IDsiClient>();
        services.TryAddSingleton<TestUserOrganisation>();
        services.AddScoped<IUserService, MockDsiUserService>();
        services.AddScoped<IDsiClient, MockDsiApiService>();
        services.RemoveAll<IInternalStaffChecker>();
        services.AddSingleton<IInternalStaffChecker, TestInternalStaffChecker>();

        return services;
    }
}
