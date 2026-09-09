using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Data.Repositories;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Test.Integration.Setup;

public class InMemoryRepositoryIntegrationTestsWebApplicationFactory : IntegrationTestsWebApplicationFactory
{
    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<IEstablishmentRepository>();
        services.RemoveAll<ISimilarSchoolsPrimaryRepository>();
        services.RemoveAll<ISimilarSchoolsSecondaryRepository>();
        services.RemoveAll<IKs2PerformanceRepository>();
        services.RemoveAll<IKs4PerformanceRepository>();
        services.RemoveAll<IKs4DestinationsRepository>();
        services.RemoveAll<IAbsenceRepository>();
        services.RemoveAll<IRiseResourcesRepository>();

        services.AddSingleton<IEstablishmentRepository, InMemoryEstablishmentRepository>();
        services.AddSingleton<ISimilarSchoolsPrimaryRepository, InMemorySimilarSchoolsPrimaryRepository>();
        services.AddSingleton<ISimilarSchoolsSecondaryRepository, InMemorySimilarSchoolsSecondaryRepository>();
        services.AddSingleton<IKs2PerformanceRepository, InMemoryKs2PerformanceRepository>();
        services.AddSingleton<IKs4PerformanceRepository, InMemoryKs4PerformanceRepository>();
        services.AddSingleton<IKs4DestinationsRepository, InMemoryKs4DestinationsRepository>();
        services.AddSingleton<IAbsenceRepository, InMemoryAbsenceRepository>();
        services.AddSingleton<IRiseResourcesRepository, InMemoryRiseResourcesRepository>();

        return services;
    }
}