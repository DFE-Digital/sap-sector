using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Core.Authentication;
using SAPSec.Web.Authentication;

namespace SAPSec.Web.Authentication;

public static class ServiceCollectionExtensions
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.Configure<AuthenticationSettings>(configuration.GetSection(AuthenticationSettings.SectionName));
        var authenticationConfig = configuration.GetSection(AuthenticationSettings.SectionName).Get<AuthenticationSettings>();

        if (authenticationConfig?.BypassAuthentication is true
            // Failsafe: Regardless of the config setting we never want to bypass authentication on production
            && !environment.IsProduction())
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = AutoAuthenticationHandler.SchemeName;
                options.DefaultAuthenticateScheme = AutoAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = AutoAuthenticationHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, AutoAuthenticationHandler>(AutoAuthenticationHandler.SchemeName, null);

            services.TryAddSingleton<TestUserOrganisation>();
            services.AddScoped<IUserService, MockDsiUserService>();
            services.AddScoped<IDsiClient, MockDsiApiService>();
        }
        else
        {
            services.AddDsiAuthentication(configuration);
        }
    }
}
