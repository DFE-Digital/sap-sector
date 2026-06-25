using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.KS4.Destinations;
using SAPSec.Data.Dto.KS4.Performance;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Store;

namespace SAPSec.Infrastructure.Json;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonDependencies(this IServiceCollection services)
    {
        // JSON files
        services.RemoveAll<IEstablishmentStore>();
        services.RemoveAll<ISimilarSchoolsSecondaryStore>();
        services.RemoveAll<IKs4PerformanceStore>();
        services.RemoveAll<IKs4DestinationsStore>();
        services.RemoveAll<IAbsenceStore>();

        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryGroupsEntry>, JsonFile<SimilarSchoolsSecondaryGroupsEntry>>();
        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryValuesEntry>, JsonFile<SimilarSchoolsSecondaryValuesEntry>>();
        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry>, JsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry>>();
        services.AddSingleton<IJsonFile<Establishment>, JsonFile<Establishment>>();
        services.AddSingleton<IJsonFile<EstablishmentEmail>, JsonFile<EstablishmentEmail>>();
        services.AddSingleton<IJsonFile<EstablishmentPerformance>, JsonFile<EstablishmentPerformance>>();
        services.AddSingleton<IJsonFile<EstablishmentAbsence>, JsonFile<EstablishmentAbsence>>();
        services.AddSingleton<IJsonFile<EstablishmentDestinations>, JsonFile<EstablishmentDestinations>>();
        services.AddSingleton<IJsonFile<LAPerformance>, JsonFile<LAPerformance>>();
        services.AddSingleton<IJsonFile<LAAbsence>, JsonFile<LAAbsence>>();
        services.AddSingleton<IJsonFile<LADestinations>, JsonFile<LADestinations>>();
        services.AddSingleton<IJsonFile<EnglandPerformance>, JsonFile<EnglandPerformance>>();
        services.AddSingleton<IJsonFile<EnglandAbsence>, JsonFile<EnglandAbsence>>();
        services.AddSingleton<IJsonFile<EnglandDestinations>, JsonFile<EnglandDestinations>>();

        services.AddSingleton<IEstablishmentStore, JsonEstablishmentStore>();
        services.AddSingleton<ISimilarSchoolsSecondaryStore, JsonSimilarSchoolsSecondaryStore>();
        services.AddSingleton<IKs4PerformanceStore, JsonKs4PerformanceStore>();
        services.AddSingleton<IKs4DestinationsStore, JsonKs4DestinationsStore>();
        services.AddSingleton<IAbsenceStore, JsonAbsenceStore>();

        return services;
    }
}
