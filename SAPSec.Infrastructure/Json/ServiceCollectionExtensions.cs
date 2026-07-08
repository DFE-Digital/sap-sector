using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Store;
using KS2 = SAPSec.Data.Dto.KS2;
using KS4 = SAPSec.Data.Dto.KS4;

namespace SAPSec.Infrastructure.Json;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonDependencies(this IServiceCollection services)
    {
        // JSON files
        services.RemoveAll<IEstablishmentStore>();
        services.RemoveAll<ISimilarSchoolsPrimaryStore>();
        services.RemoveAll<ISimilarSchoolsSecondaryStore>();
        services.RemoveAll<IKs2PerformanceStore>();
        services.RemoveAll<IKs4PerformanceStore>();
        services.RemoveAll<IKs4DestinationsStore>();
        services.RemoveAll<IAbsenceStore>();

        services.AddSingleton<IJsonFile<Establishment>, JsonFile<Establishment>>();
        services.AddSingleton<IJsonFile<EstablishmentEmail>, JsonFile<EstablishmentEmail>>();

        services.AddSingleton<IJsonFile<SimilarSchoolsPrimaryGroupsEntry>, JsonFile<SimilarSchoolsPrimaryGroupsEntry>>();
        services.AddSingleton<IJsonFile<SimilarSchoolsPrimaryValuesEntry>, JsonFile<SimilarSchoolsPrimaryValuesEntry>>();
        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryGroupsEntry>, JsonFile<SimilarSchoolsSecondaryGroupsEntry>>();
        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryValuesEntry>, JsonFile<SimilarSchoolsSecondaryValuesEntry>>();
        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry>, JsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry>>();

        services.AddSingleton<IJsonFile<KS2.Performance.EstablishmentPerformance>, JsonFile<KS2.Performance.EstablishmentPerformance>>();
        services.AddSingleton<IJsonFile<KS2.Performance.LAPerformance>, JsonFile<KS2.Performance.LAPerformance>>();
        services.AddSingleton<IJsonFile<KS2.Performance.EnglandPerformance>, JsonFile<KS2.Performance.EnglandPerformance>>();

        services.AddSingleton<IJsonFile<KS4.Performance.EstablishmentPerformance>, JsonFile<KS4.Performance.EstablishmentPerformance>>();
        services.AddSingleton<IJsonFile<KS4.Performance.LAPerformance>, JsonFile<KS4.Performance.LAPerformance>>();
        services.AddSingleton<IJsonFile<KS4.Performance.EnglandPerformance>, JsonFile<KS4.Performance.EnglandPerformance>>();

        services.AddSingleton<IJsonFile<KS4.Destinations.EstablishmentDestinations>, JsonFile<KS4.Destinations.EstablishmentDestinations>>();
        services.AddSingleton<IJsonFile<KS4.Destinations.LADestinations>, JsonFile<KS4.Destinations.LADestinations>>();
        services.AddSingleton<IJsonFile<KS4.Destinations.EnglandDestinations>, JsonFile<KS4.Destinations.EnglandDestinations>>();

        services.AddSingleton<IJsonFile<EstablishmentAbsence>, JsonFile<EstablishmentAbsence>>();
        services.AddSingleton<IJsonFile<LAAbsence>, JsonFile<LAAbsence>>();
        services.AddSingleton<IJsonFile<EnglandAbsence>, JsonFile<EnglandAbsence>>();

        services.AddSingleton<IEstablishmentStore, JsonEstablishmentStore>();
        services.AddSingleton<ISimilarSchoolsPrimaryStore, JsonSimilarSchoolsPrimaryStore>();
        services.AddSingleton<ISimilarSchoolsSecondaryStore, JsonSimilarSchoolsSecondaryStore>();
        services.AddSingleton<IKs2PerformanceStore, JsonKs2PerformanceStore>();
        services.AddSingleton<IKs4PerformanceStore, JsonKs4PerformanceStore>();
        services.AddSingleton<IKs4DestinationsStore, JsonKs4DestinationsStore>();
        services.AddSingleton<IAbsenceStore, JsonAbsenceStore>();

        return services;
    }
}
