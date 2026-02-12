using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Services;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Infrastructure.Repositories.Json;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class DependenciesExtensions
    {
        public static void AddDependencies(this IServiceCollection services)
        {
            // JSON files
            services.AddSingleton<JsonFile<SimilarSchoolsSecondaryGroupsRow>>();
            services.AddSingleton<JsonFile<SimilarSchoolsSecondaryValuesRow>>();
            services.AddSingleton<JsonFile<Establishment>>();
            services.AddSingleton<JsonFile<EstablishmentPerformance>>();
            services.AddSingleton<JsonFile<Lookup>>();

            services.AddSingleton<IEstablishmentService, EstablishmentService>();

            services.AddSingleton<ILookupRepository, LookupRepository>();
            services.AddSingleton<ILookupService, LookupService>();

            services.AddSingleton<ISchoolSearchIndexReader, LuceneShoolSearchIndexReader>();

            services.AddSingleton<ISchoolSearchService, SchoolSearchService>();

            // Register SchoolDetailsService with explicit rule dependencies
            services.AddSingleton<ISchoolDetailsService, SchoolDetailsService>();

            // Use cases
            services.AddSingleton<FindSimilarSchools>();
            services.AddSingleton<GetSimilarSchoolDetails>();
        }
    }
}
