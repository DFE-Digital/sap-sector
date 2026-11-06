using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Core.Caching;
using SAPSec.Data.Repositories;
using SAPSec.Test.Common.InMemory;
using SAPSec.Web.Extensions;

namespace SAPSec.Test.Integration.Setup;

public class InMemoryRepositoryIntegrationTestsWebApplicationFactory : IntegrationTestsWebApplicationFactory
{
    protected override IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNestedCache<RequestCache, InMemoryCache>();

        services.RemoveAll<IEstablishmentRepository>();
        services.RemoveAll<ISimilarSchoolsPrimaryRepository>();
        services.RemoveAll<ISimilarSchoolsSecondaryRepository>();
        services.RemoveAll<IKs2PerformanceRepository>();
        services.RemoveAll<IKs4PerformanceRepository>();
        services.RemoveAll<IKs4DestinationsRepository>();
        services.RemoveAll<IAbsenceRepository>();
        services.RemoveAll<IExclusionsRepository>();
        services.RemoveAll<IRiseResourcesRepository>();

        services.AddRepository<IEstablishmentRepository, InMemoryEstablishmentRepository, CachedEstablishmentRepository>(configuration);
        services.AddRepository<ISimilarSchoolsPrimaryRepository, InMemorySimilarSchoolsPrimaryRepository, CachedSimilarSchoolsPrimaryRepository>(configuration);
        services.AddRepository<ISimilarSchoolsSecondaryRepository, InMemorySimilarSchoolsSecondaryRepository, CachedSimilarSchoolsSecondaryRepository>(configuration);
        services.AddRepository<IKs2PerformanceRepository, InMemoryKs2PerformanceRepository, CachedKs2PerformanceRepository>(configuration);
        services.AddRepository<IKs4PerformanceRepository, InMemoryKs4PerformanceRepository, CachedKs4PerformanceRepository>(configuration);
        services.AddRepository<IKs4DestinationsRepository, InMemoryKs4DestinationsRepository, CachedKs4DestinationsRepository>(configuration);
        services.AddRepository<IAbsenceRepository, InMemoryAbsenceRepository, CachedAbsenceRepository>(configuration);
        services.AddRepository<IExclusionsRepository, InMemoryExclusionsRepository, CachedExclusionsRepository>(configuration);
        services.AddRepository<IRiseResourcesRepository, InMemoryRiseResourcesRepository, CachedRiseResourcesRepository>(configuration);

        return services;
    }
}