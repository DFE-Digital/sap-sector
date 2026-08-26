using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services;
using SAPSec.Core.UseCases;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddUseCases(this IServiceCollection services)
    {
        // School info

        services.AddSingleton<IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse>, GetSchoolInfoUseCase>();

        // School details

        services.AddSingleton<GetSimilarSchoolDetails>();
        services.AddSingleton<IUseCase<GetPrimarySimilarSchoolDetailsRequest, GetPrimarySimilarSchoolDetailsResponse>, GetPrimarySimilarSchoolDetailsUseCase>();

        // Similar schools

        services.AddSingleton<FindSimilarSchools>();
        services.AddSingleton<IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse>, FindPrimarySimilarSchoolsUseCase>();
        services.AddSingleton<GetCharacteristicsComparison>();
        services.AddSingleton<GetPrimaryCharacteristicsComparison>();

        // Measures

        services.AddSingleton<IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse>, GetSchoolKs2PerformanceMeasuresUseCase>();
        services.AddSingleton<IUseCase<GetComparisonKs2PerformanceMeasuresRequest, GetComparisonKs2PerformanceMeasuresResponse>, GetComparisonKs2PerformanceMeasuresUseCase>();

        services.AddSingleton<IUseCase<GetSchoolKs4HeadlineMeasuresRequest, GetSchoolKs4HeadlineMeasuresResponse>, GetSchoolKs4HeadlineMeasuresUseCase>();
        services.AddSingleton<IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse>, GetComparisonKs4HeadlineMeasuresUseCase>();

        services.AddSingleton<IUseCase<GetSchoolKs4CoreSubjectsRequest, GetSchoolKs4CoreSubjectsResponse>, GetSchoolKs4CoreSubjectsUseCase>();
        services.AddSingleton<IUseCase<GetComparisonKs4CoreSubjectsRequest, GetComparisonKs4CoreSubjectsResponse>, GetComparisonKs4CoreSubjectsUseCase>();

        services.AddSingleton<GetAttendanceMeasures>();
        services.AddSingleton<IUseCase<GetSchoolAttendanceMeasuresRequest, GetSchoolAttendanceMeasuresResponse>, GetSchoolAttendanceMeasuresUseCase>();
        services.AddSingleton<IUseCase<GetComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse>, GetComparisonAttendanceMeasuresUseCase>();
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<ISchoolSearchService, SchoolSearchService>();
        services.AddSingleton<ISchoolDetailsService, SchoolDetailsService>();
    }
}
