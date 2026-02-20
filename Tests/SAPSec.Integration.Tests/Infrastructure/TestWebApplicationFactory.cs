using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Infrastructure.Repositories.Json;
using SAPSec.Infrastructure.Repositories;
using SAPSec.Integration.Tests.Mocks;
using SAPSec.Web;

namespace SAPSec.Integration.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://127.0.0.1:0", "https://127.0.0.1:0");

        builder.UseEnvironment("IntegrationTests");

        var testDataFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "Establishments-Integration-Test-Data.csv");

        if (!File.Exists(testDataFilePath)) throw new FileNotFoundException("Test data file not found", testDataFilePath);

        var configurationValues = new Dictionary<string, string?>
        {
            { "Establishments:CsvPath", testDataFilePath },
			{ "DsiConfiguration:ClientId", TestValues.ClientId },
            { "DsiConfiguration:ClientSecret", TestValues.ClientSecret },
            { "DsiConfiguration:Authority", TestValues.Authority },
            { "DsiConfiguration:RequireHttpsMetadata", "false" },
            { "DsiConfiguration:ValidateIssuer", "false" },
            { "DsiConfiguration:ValidateAudience", "false" },
            { "DsiConfiguration:ApiUri", TestValues.ApiUri },
            { "DsiConfiguration:ApiSecret", TestValues.ApiSecret },
            { "DsiConfiguration:Audience", TestValues.Audience },
            { "DsiConfiguration:TokenExpiryMinutes", TestValues.TokenExpiryMinutes }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        builder
            // This configuration is used during the creation of the application
            // (e.g. BEFORE WebApplication.CreateBuilder(args) is called in Program.cs).
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(configurationValues);
            })
            .ConfigureServices(services =>
            {
                // Add or replace any services that the application needs during testing.
				services.RemoveAll<IUserService>();
		        services.RemoveAll<IDsiClient>();
				services.AddScoped<IUserService, MockDsiUserService>();
        		services.AddScoped<IDsiClient, MockDsiApiService>();
                
                services.RemoveAll<IEstablishmentRepository>();
                services.RemoveAll<ISimilarSchoolsSecondaryRepository>();

                services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryGroupsRow>, JsonFile<SimilarSchoolsSecondaryGroupsRow>>();
                services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryValuesRow>, JsonFile<SimilarSchoolsSecondaryValuesRow>>();
                services.AddSingleton<IJsonFile<Establishment>, JsonFile<Establishment>>();
                services.AddSingleton<IJsonFile<EstablishmentPerformance>, JsonFile<EstablishmentPerformance>>();

                services.AddScoped<IEstablishmentRepository, JsonEstablishmentRepository>();
                services.AddScoped<ISimilarSchoolsSecondaryRepository, JsonSimilarSchoolsSecondaryRepository>();

            });
    }

    private static class TestValues
    {
        // DSI Test Values
        public const string ClientId = "test-client-id";
        public const string ClientSecret = "test-client-secret";
        public const string Authority = "https://test-oidc.signin.education.gov.uk";
        public const string ApiUri = "https://test-api.signin.education.gov.uk";
        public const string ApiSecret = "test-api-secret";
        public const string Audience = "test-audience";
        public const string TokenExpiryMinutes = "60";
    }
}
