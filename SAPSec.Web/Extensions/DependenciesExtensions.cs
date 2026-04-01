using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;
using SAPSec.Core.Services;
using SAPSec.Infrastructure.Json;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Web.Formatters;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DependenciesExtensions
{
    public static void AddDependencies(this IServiceCollection services)
    {
        // JSON files
        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryGroupsEntry>, JsonFile<SimilarSchoolsSecondaryGroupsEntry>>();
        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryValuesEntry>, JsonFile<SimilarSchoolsSecondaryValuesEntry>>();
        services.AddSingleton<IJsonFile<Establishment>, JsonFile<Establishment>>();
        services.AddSingleton<IJsonFile<EstablishmentPerformance>, JsonFile<EstablishmentPerformance>>();
        services.AddSingleton<IJsonFile<LAPerformance>, JsonFile<LAPerformance>>();
        services.AddSingleton<IJsonFile<EnglandPerformance>, JsonFile<EnglandPerformance>>();
        services.AddSingleton<IJsonFile<EstablishmentDestinations>, JsonFile<EstablishmentDestinations>>();
        services.AddSingleton<IJsonFile<LADestinations>, JsonFile<LADestinations>>();
        services.AddSingleton<IJsonFile<EnglandDestinations>, JsonFile<EnglandDestinations>>();
        services.AddSingleton<IJsonFile<Lookup>, JsonFile<Lookup>>();
        services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry>, JsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry>>();
        services.AddSingleton<ISchoolSearchIndexReader, LuceneShoolSearchIndexReader>();
        services.AddSingleton<ISchoolSearchService, SchoolSearchService>();

        // Register SchoolDetailsService with explicit rule dependencies
        services.AddSingleton<ISchoolDetailsService, SchoolDetailsService>();

        // Use cases
        services.AddSingleton<GetKs4HeadlineMeasures>();
        services.AddSingleton<GetSchoolKs4HeadlineMeasures>();
        services.AddSingleton<GetSchoolKs4CoreSubjects>();
        services.AddSingleton<GetAttendanceMeasures>();
        services.AddSingleton<FindSimilarSchools>();
        services.AddSingleton<GetSimilarSchoolDetails>();
        services.AddSingleton<GetCharacteristicsComparison>();

        // Formatters
        services.AddSingleton<ICharacteristicsComparisonFormatter, CharacteristicsComparisonFormatter>();
    }
}
