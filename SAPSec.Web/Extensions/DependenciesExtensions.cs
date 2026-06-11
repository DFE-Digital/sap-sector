using SAPSec.Core.School.Attendance;
using SAPSec.Core.School.Details;
using SAPSec.Core.School.Info;
using SAPSec.Core.School.Search;
using SAPSec.Core.School.Secondary;
using SAPSec.Core.School.Secondary.Ks4CoreSubjects;
using SAPSec.Core.School.Secondary.Ks4HeadlineMeasures;
using SAPSec.Core.School.Similarity;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Web.Formatters;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DependenciesExtensions
{
    public static void AddDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ISchoolSearchIndexReader, LuceneShoolSearchIndexReader>();
        services.AddSingleton<ISchoolSearchService, SchoolSearchService>();
        services.AddSingleton<ISchoolDetailsService, SchoolDetailsService>();
        services.AddSingleton<ISecondarySchoolsRepository, SecondarySchoolsRepository>();

        // Use cases
        services.AddSingleton<GetSchoolKs4HeadlineMeasuresUseCase>();
        services.AddSingleton<GetSchoolComparisonKs4HeadlineMeasuresUseCase>();
        services.AddSingleton<GetSchoolKs4CoreSubjectsUseCase>();
        services.AddSingleton<GetSchoolComparisonKs4CoreSubjectsUseCase>();
        services.AddSingleton<GetAttendanceMeasuresUseCase>();
        services.AddSingleton<FindSimilarSchoolsUseCase>();
        services.AddSingleton<GetSimilarSchoolDetailsUseCase>();
        services.AddSingleton<GetCharacteristicsComparisonUseCase>();
        services.AddSingleton<GetSchoolInfoUseCase>();
        services.AddSingleton<GetSchoolDetailsUseCase>();
        services.AddSingleton<GetSearchResultsUseCase>();

        // Formatters
        services.AddSingleton<ICharacteristicsComparisonFormatter, CharacteristicsComparisonFormatter>();
    }
}
