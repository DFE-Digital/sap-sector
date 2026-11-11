using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Caching;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.RiseResources;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolDetails.Comparison;
using SAPSec.Core.Features.SchoolDetails.School;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Time;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Core;

public static class ServiceKeys
{
    public const string RealImplementation = nameof(RealImplementation);
    public const string Primary = nameof(Primary);
    public const string Secondary = nameof(Secondary);
}

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddUseCases(this IServiceCollection services, IConfiguration configuration)
    {
        // School info

        services.AddUseCase<GetSchoolInfoUseCase>(configuration);

        // School details

        services.AddUseCase<GetSchoolDetailsUseCase>(configuration);
        services.AddKeyedUseCase<GetPrimaryComparisonSchoolDetailsUseCase>(ServiceKeys.Primary, configuration);
        services.AddKeyedUseCase<GetSecondaryComparisonSchoolDetailsUseCase>(ServiceKeys.Secondary, configuration);

        // Similar schools

        services.AddUseCase<FindPrimarySimilarSchoolsUseCase>(configuration);
        services.AddUseCase<FindSecondarySimilarSchoolsUseCase>(configuration);
        services.AddUseCase<GetSecondaryComparisonSimilarityCharacteristicsUseCase>(configuration);
        services.AddUseCase<GetPrimaryComparisonSimilarityCharacteristicsUseCase>(configuration);
        services.AddUseCase<GetAllThroughSimilarSchoolPhasesUseCase>(configuration);

        // Measures

        services.AddUseCase<GetSchoolKs2PerformanceMeasuresUseCase>(configuration);
        services.AddUseCase<GetComparisonKs2PerformanceMeasuresUseCase>(configuration);

        services.AddUseCase<GetSchoolKs4HeadlineMeasuresUseCase>(configuration);
        services.AddUseCase<GetComparisonKs4HeadlineMeasuresUseCase>(configuration);

        services.AddUseCase<GetSchoolKs4CoreSubjectsMeasuresUseCase>(configuration);
        services.AddUseCase<GetComparisonKs4CoreSubjectsMeasuresUseCase>(configuration);

        services.AddUseCase<GetSchoolAttendanceMeasuresUseCase>(configuration);
        services.AddUseCase<GetPrimaryComparisonAttendanceMeasuresUseCase>(configuration);
        services.AddUseCase<GetSecondaryComparisonAttendanceMeasuresUseCase>(configuration);

        // RISE Resources

        services.AddUseCase<GetRiseResourcesUseCase>(configuration);

        // Time

        services.AddSingleton<ICurrentTimeProvider, CurrentTimeProvider>();
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<ISchoolSearchService, SchoolSearchService>();
        services.AddSingleton<ISchoolDetailsService, SchoolDetailsService>();
        services.AddSingleton<ISchoolClosureEligibilityService, SchoolClosureEligibilityService>();
        services.AddSingleton<ISchoolPredecessorRelationshipService, SchoolPredecessorRelationshipService>();
    }
}
