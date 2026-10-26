using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
using SAPSec.Core.UseCases;
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
    public static void AddUseCases(this IServiceCollection services)
    {
        // School info

        services.AddUseCase<GetSchoolInfoUseCase>();

        // School details

        services.AddUseCase<GetSchoolDetailsUseCase>();
        services.AddKeyedUseCase<GetPrimaryComparisonSchoolDetailsUseCase>(ServiceKeys.Primary);
        services.AddKeyedUseCase<GetSecondaryComparisonSchoolDetailsUseCase>(ServiceKeys.Secondary);

        // Similar schools

        services.AddUseCase<FindPrimarySimilarSchoolsUseCase>();
        services.AddUseCase<FindSecondarySimilarSchoolsUseCase>();
        services.AddUseCase<GetSecondaryComparisonSimilarityCharacteristicsUseCase>();
        services.AddUseCase<GetPrimaryComparisonSimilarityCharacteristicsUseCase>();
        services.AddUseCase<GetAllThroughSimilarSchoolPhasesUseCase>();

        // Measures

        services.AddUseCase<GetSchoolKs2PerformanceMeasuresUseCase>();
        services.AddUseCase<GetComparisonKs2PerformanceMeasuresUseCase>();

        services.AddUseCase<GetSchoolKs4HeadlineMeasuresUseCase>();
        services.AddUseCase<GetComparisonKs4HeadlineMeasuresUseCase>();

        services.AddUseCase<GetSchoolKs4CoreSubjectsMeasuresUseCase>();
        services.AddUseCase<GetComparisonKs4CoreSubjectsMeasuresUseCase>();

        services.AddUseCase<GetSchoolAttendanceMeasuresUseCase>();
        services.AddUseCase<GetPrimaryComparisonAttendanceMeasuresUseCase>();
        services.AddUseCase<GetSecondaryComparisonAttendanceMeasuresUseCase>();

        // RISE Resources

        services.AddUseCase<GetRiseResourcesUseCase>();

        // Time

        services.AddSingleton<ICurrentTimeProvider, CurrentTimeProvider>();

        // Cache

        services.AddSingleton<ICache>(sp => new NestedCache(
            new RequestCache(sp.GetRequiredService<IHttpContextAccessor>()),
            new InMemoryCache()));
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<ISchoolSearchService, SchoolSearchService>();
        services.AddSingleton<ISchoolDetailsService, SchoolDetailsService>();
        services.AddSingleton<ISchoolClosureEligibilityService, SchoolClosureEligibilityService>();
        services.AddSingleton<ISchoolPredecessorRelationshipService, SchoolPredecessorRelationshipService>();
    }

    private static void AddUseCase<TImplementation>(this IServiceCollection services)
        where TImplementation : class
        => services.AddUseCase(typeof(TImplementation));

    private static void AddUseCase(this IServiceCollection services, Type implementationType)
    {
        var interfaceType = GetInterfaceType(implementationType);

        services.AddKeyedSingleton(interfaceType, ServiceKeys.RealImplementation, implementationType);
        services.AddSingleton(interfaceType, sp =>
            sp.GetCachedUseCase(interfaceType, ServiceKeys.RealImplementation));
    }

    private static void AddKeyedUseCase<TImplementation>(this IServiceCollection services, string serviceKey)
        where TImplementation : class
        => services.AddKeyedUseCase(serviceKey, typeof(TImplementation));

    private static void AddKeyedUseCase(this IServiceCollection services, string serviceKey, Type implementationType)
    {
        var interfaceType = GetInterfaceType(implementationType);

        services.AddKeyedSingleton(interfaceType, $"{ServiceKeys.RealImplementation}|{serviceKey}", implementationType);
        services.AddKeyedSingleton(interfaceType, serviceKey, (sp, k) =>
            sp.GetCachedUseCase(interfaceType, $"{ServiceKeys.RealImplementation}|{k}"));
    }

    private static Type GetInterfaceType(Type implementationType)
    {
        var interfaceTypes = implementationType.FindInterfaces((t, _) =>
            t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(IUseCase<,>)), null);

        if (interfaceTypes.Length != 1)
        {
            throw new ApplicationException($"Use case type should only have a single IUseCase definition: {implementationType.FullName}");
        }

        var interfaceType = interfaceTypes.First();

        if (interfaceType.GenericTypeArguments.Length != 2)
        {
            throw new ApplicationException($"Use case interface type should have two type parameters: {interfaceType.FullName}");
        }

        return interfaceType;
    }

    private static object GetCachedUseCase(this IServiceProvider sp, Type interfaceType, string serviceKey)
    {
        var useCase = sp.GetKeyedService(interfaceType, serviceKey);

        if (useCase is null)
        {
            throw new ApplicationException($"No use case implemention defined for interface: {interfaceType.FullName}");
        }

        var cachedUseCaseType = typeof(CachedUseCase<,>).MakeGenericType(interfaceType.GenericTypeArguments);

        var cache = sp.GetRequiredService<ICache>();
        var cachedUseCase = Activator.CreateInstance(cachedUseCaseType, useCase, cache);

        if (cachedUseCase is null)
        {
            throw new ApplicationException($"Instance was null: {cachedUseCaseType.FullName}");
        }

        return cachedUseCase;
    }
}
