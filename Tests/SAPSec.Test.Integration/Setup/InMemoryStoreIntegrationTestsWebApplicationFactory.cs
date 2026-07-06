using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Data.Store;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Test.Integration.Setup;

public class InMemoryStoreIntegrationTestsWebApplicationFactory : IntegrationTestsWebApplicationFactory
{
    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<IEstablishmentStore>();
        services.RemoveAll<ISimilarSchoolsPrimaryStore>();
        services.RemoveAll<ISimilarSchoolsSecondaryStore>();
        services.RemoveAll<IKs2PerformanceStore>();
        services.RemoveAll<IKs4PerformanceStore>();
        services.RemoveAll<IKs4DestinationsStore>();
        services.RemoveAll<IAbsenceStore>();

        services.AddSingleton<IEstablishmentStore, InMemoryEstablishmentStore>();
        services.AddSingleton<ISimilarSchoolsPrimaryStore, InMemorySimilarSchoolsPrimaryStore>();
        services.AddSingleton<ISimilarSchoolsSecondaryStore, InMemorySimilarSchoolsSecondaryStore>();
        services.AddSingleton<IKs2PerformanceStore, InMemoryKs2PerformanceStore>();
        services.AddSingleton<IKs4PerformanceStore, InMemoryKs4PerformanceStore>();
        services.AddSingleton<IKs4DestinationsStore, InMemoryKs4DestinationsStore>();
        services.AddSingleton<IAbsenceStore, InMemoryAbsenceStore>();

        return services;
    }
}