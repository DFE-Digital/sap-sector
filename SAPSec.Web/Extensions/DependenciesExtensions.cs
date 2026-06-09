using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Ks4CoreSubjects;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SimilarSchools;
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
