using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Store;

namespace SAPSec.Test.Common.InMemoryStore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryStoreDependencies(this IServiceCollection services)
    {
        //services.RemoveAll<IEstablishmentStore>();
        //services.RemoveAll<ISimilarSchoolsSecondaryStore>();
        //services.RemoveAll<IKs4PerformanceStore>();
        //services.RemoveAll<IKs4DestinationsStore>();
        //services.RemoveAll<IAbsenceStore>();

        services.AddSingleton<IEstablishmentStore, InMemoryEstablishmentStore>();
        services.AddSingleton<ISimilarSchoolsSecondaryStore, InMemorySimilarSchoolsSecondaryStore>();
        services.AddSingleton<IKs4PerformanceStore, InMemoryKs4PerformanceStore>();
        services.AddSingleton<IKs4DestinationsStore, InMemoryKs4DestinationsStore>();
        services.AddSingleton<IAbsenceStore, InMemoryAbsenceStore>();

        return services;
    }
}