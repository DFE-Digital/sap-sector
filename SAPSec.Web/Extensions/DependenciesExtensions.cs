using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Web.Formatters;
using SAPSec.Web.Services;
using System.Diagnostics.CodeAnalysis;

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

        // Formatters
        services.AddSingleton<ICharacteristicsComparisonFormatter, CharacteristicsComparisonFormatter>();
    }
}
