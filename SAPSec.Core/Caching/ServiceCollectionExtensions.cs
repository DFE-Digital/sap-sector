using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public static class ServiceCollectionExtensions
{
    public static void AddRepository<TInterface, TRealImplementation, TCachedImplementation>(this IServiceCollection services, IConfiguration configuration)
        where TInterface : class, IRepository
        where TRealImplementation : class, TInterface
        where TCachedImplementation : class, TInterface
    {
        var config = configuration.GetSection(nameof(CachingConfiguration)).Get<CachingConfiguration>();

        if (config is null || !config.EnableCaching || !config.CacheRepositories)
        {
            services.AddSingleton<TInterface, TRealImplementation>();
            return;
        }

        services.AddKeyedSingleton<TInterface, TRealImplementation>(ServiceKeys.RealImplementation);
        services.AddSingleton<TInterface, TCachedImplementation>();
    }

    public static void AddUseCase<TImplementation>(this IServiceCollection services, IConfiguration configuration)
        where TImplementation : class
        => services.AddUseCase(typeof(TImplementation), configuration);

    private static void AddUseCase(this IServiceCollection services, Type implementationType, IConfiguration configuration)
    {
        var config = configuration.GetSection(nameof(CachingConfiguration)).Get<CachingConfiguration>();

        var interfaceType = GetInterfaceType(implementationType);

        if (config is null || !config.EnableCaching || !config.CacheUseCases)
        {
            services.AddSingleton(interfaceType, implementationType);
            return;
        }

        services.AddKeyedSingleton(interfaceType, ServiceKeys.RealImplementation, implementationType);
        services.AddSingleton(interfaceType, sp =>
            sp.GetCachedUseCase(interfaceType, ServiceKeys.RealImplementation));
    }

    public static void AddKeyedUseCase<TImplementation>(this IServiceCollection services, string serviceKey, IConfiguration configuration)
        where TImplementation : class
        => services.AddKeyedUseCase(serviceKey, typeof(TImplementation), configuration);

    public static void AddKeyedUseCase(this IServiceCollection services, string serviceKey, Type implementationType, IConfiguration configuration)
    {
        var config = configuration.GetSection(nameof(CachingConfiguration)).Get<CachingConfiguration>();

        var interfaceType = GetInterfaceType(implementationType);

        if (config is null || !config.EnableCaching || !config.CacheUseCases)
        {
            services.AddKeyedSingleton(interfaceType, serviceKey, implementationType);
            return;
        }

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
