using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;
using SAPSec.Core.Model.Generated.Absence;
using SAPSec.Core.Model.Generated.KS4.Destinations;
using SAPSec.Core.Model.Generated.KS4.Performance;
using SAPSec.Core.Model.Generated.SimilarSchools.Secondary;

namespace SAPSec.Infrastructure.Json;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonDependencies(this IServiceCollection services)
    {
        // JSON files
        services.RemoveAll<IEstablishmentRepository>();
        services.RemoveAll<ISimilarSchoolsSecondaryRepository>();
        services.RemoveAll<IKs4PerformanceRepository>();
        services.RemoveAll<IKs4DestinationsRepository>();
        services.RemoveAll<IAbsenceRepository>();

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

        services.AddSingleton<IEstablishmentRepository, JsonEstablishmentRepository>();
        services.AddSingleton<ISimilarSchoolsSecondaryRepository, JsonSimilarSchoolsSecondaryRepository>();
        services.AddSingleton<IKs4PerformanceRepository, JsonKs4PerformanceRepository>();
        services.AddSingleton<IKs4DestinationsRepository, JsonKs4DestinationsRepository>();
        services.AddSingleton<IAbsenceRepository, JsonAbsenceRepository>();

        return services;
    }
}
