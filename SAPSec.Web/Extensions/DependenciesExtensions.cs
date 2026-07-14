using SAPSec.Core.School.Similarity.UseCases;
using SAPSec.Core.School.Info;
using SAPSec.Core.School.Primary;
using SAPSec.Core.School.Search;
using SAPSec.Core.School.Secondary.Ks4CoreSubjects.UseCases;
using SAPSec.Core.School.Secondary.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto.KS2.Performance;
using SAPSec.Data.Repositories;
using SAPSec.Infrastructure.Json;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Web.Formatters;
using SAPSec.Web.Services;
using System.Diagnostics.CodeAnalysis;
using SAPSec.Core.School.Attendance;
using SAPSec.Core.School.Details;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DependenciesExtensions
{
    public static void AddDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ISchoolSearchIndexReader, LuceneShoolSearchIndexReader>();
        services.AddScoped<ISchoolSearchService, SchoolSearchService>();
        services.AddSingleton<ISchoolDetailsService, SchoolDetailsService>();
        services.AddScoped<IRequestSchoolAccessor, RequestSchoolAccessor>();

        // Use cases
        services.AddSingleton<GetKs4HeadlineMeasures>();
        services.AddSingleton<GetSchoolKs4HeadlineMeasures>();
        services.AddSingleton<GetSchoolKs4CoreSubjects>();
        services.AddSingleton<GetFilteredSchoolKs4CoreSubject>();
        services.AddSingleton<GetAttendanceMeasures>();
        services.AddSingleton<FindSimilarSchools>();
        services.AddSingleton<GetSimilarSchoolDetails>();
        services.AddSingleton<GetCharacteristicsComparison>();
        services.AddSingleton<IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse>, GetSchoolInfoUseCase>();
        services.AddSingleton<IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse>, GetSchoolKs2PerformanceMeasuresUseCase>();

        services.AddSingleton<IJsonFileFactory, JsonFileFactory>();
        services.AddJsonFile<EstablishmentPerformance>(JsonDataSource.PrimarySchools);
        services.AddJsonFile<LAPerformance>(JsonDataSource.PrimarySchools);
        services.AddJsonFile<EnglandPerformance>(JsonDataSource.PrimarySchools);
        services.AddSingleton<IKs2PerformanceRepository, JsonKs2PerformanceRepository>();

        // Formatters
        services.AddSingleton<ICharacteristicsComparisonFormatter, CharacteristicsComparisonFormatter>();
    }
}
