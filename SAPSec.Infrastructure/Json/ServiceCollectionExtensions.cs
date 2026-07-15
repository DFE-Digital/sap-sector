using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;
using KS2 = SAPSec.Data.Dto.KS2;
using KS4 = SAPSec.Data.Dto.KS4;

namespace SAPSec.Infrastructure.Json;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonDependencies(this IServiceCollection services)
    {
        services.AddSingleton<IJsonFileFactory, JsonFileFactory>();

        // JSON files
        services.RemoveAll<IEstablishmentRepository>();
        services.RemoveAll<ISimilarSchoolsPrimaryRepository>();
        services.RemoveAll<ISimilarSchoolsSecondaryRepository>();
        services.RemoveAll<IKs2PerformanceRepository>();
        services.RemoveAll<IKs4PerformanceRepository>();
        services.RemoveAll<IKs4DestinationsRepository>();
        services.RemoveAll<IAbsenceRepository>();

        services.AddJsonFile<Establishment>(JsonDataSource.Generated);
        services.AddJsonFile<EstablishmentEmail>(JsonDataSource.Generated);

        services.AddJsonFile<SimilarSchoolsPrimaryGroupsEntry>(JsonDataSource.Generated);
        services.AddJsonFile<SimilarSchoolsPrimaryValuesEntry>(JsonDataSource.Generated);
        services.AddJsonFile<SimilarSchoolsSecondaryGroupsEntry>(JsonDataSource.Generated);
        services.AddJsonFile<SimilarSchoolsSecondaryValuesEntry>(JsonDataSource.Generated);
        services.AddJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry>(JsonDataSource.Generated);

        services.AddJsonFile<KS2.Performance.EstablishmentPerformance>(JsonDataSource.PrimarySchools);
        services.AddJsonFile<KS2.Performance.LAPerformance>(JsonDataSource.PrimarySchools);
        services.AddJsonFile<KS2.Performance.EnglandPerformance>(JsonDataSource.PrimarySchools);

        services.AddJsonFile<KS4.Performance.EstablishmentPerformance>(JsonDataSource.Generated);
        services.AddJsonFile<KS4.Performance.LAPerformance>(JsonDataSource.Generated);
        services.AddJsonFile<KS4.Performance.EnglandPerformance>(JsonDataSource.Generated);

        services.AddJsonFile<KS4.Destinations.EstablishmentDestinations>(JsonDataSource.Generated);
        services.AddJsonFile<KS4.Destinations.LADestinations>(JsonDataSource.Generated);
        services.AddJsonFile<KS4.Destinations.EnglandDestinations>(JsonDataSource.Generated);

        services.AddJsonFile<EstablishmentAbsence>(JsonDataSource.Generated);
        services.AddJsonFile<LAAbsence>(JsonDataSource.Generated);
        services.AddJsonFile<EnglandAbsence>(JsonDataSource.Generated);

        services.AddSingleton<IEstablishmentRepository, JsonEstablishmentRepository>();
        services.AddSingleton<ISimilarSchoolsPrimaryRepository, JsonSimilarSchoolsPrimaryRepository>();
        services.AddSingleton<ISimilarSchoolsSecondaryRepository, JsonSimilarSchoolsSecondaryRepository>();
        services.AddSingleton<IKs2PerformanceRepository, JsonKs2PerformanceRepository>();
        services.AddSingleton<IKs4PerformanceRepository, JsonKs4PerformanceRepository>();
        services.AddSingleton<IKs4DestinationsRepository, JsonKs4DestinationsRepository>();
        services.AddSingleton<IAbsenceRepository, JsonAbsenceRepository>();

        return services;
    }

    public static IServiceCollection AddJsonFile<T>(this IServiceCollection services, JsonDataSource source) where T : class
    {
        services.AddSingleton(services => services.GetRequiredService<IJsonFileFactory>().Create<T>(source));

        return services;
    }
}
