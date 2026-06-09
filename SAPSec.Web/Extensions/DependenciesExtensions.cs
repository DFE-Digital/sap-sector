using SAPSec.Core.Attendance;
using SAPSec.Core.Ks4CoreSubjects;
using SAPSec.Core.Ks4HeadlineMeasures;
using SAPSec.Core.SchoolDetails;
using SAPSec.Core.SchoolSearch;
using SAPSec.Core.SimilarSchools;
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

        // Use cases
        services.AddSingleton<GetSchoolKs4HeadlineMeasuresUseCase>();
        services.AddSingleton<GetSchoolComparisonKs4HeadlineMeasuresUseCase>();
        services.AddSingleton<GetSchoolKs4CoreSubjectsUseCase>();
        services.AddSingleton<GetSchoolComparisonKs4CoreSubjectsUseCase>();
        services.AddSingleton<GetAttendanceMeasuresUseCase>();
        services.AddSingleton<FindSimilarSchoolsUseCase>();
        services.AddSingleton<GetSimilarSchoolDetailsUseCase>();
        services.AddSingleton<GetCharacteristicsComparisonUseCase>();

        // Formatters
        services.AddSingleton<ICharacteristicsComparisonFormatter, CharacteristicsComparisonFormatter>();
    }
}
