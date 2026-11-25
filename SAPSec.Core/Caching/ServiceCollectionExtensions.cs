using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Core.UseCases;

namespace SAPSec.Core.Caching;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds caching based on <see cref="CachingSettings"/> defined in <paramref name="configuration"/>. If use case caching is turned on,
    /// <paramref name="configure"/> is called to further configure caching behaviour as required.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration to find <see cref="CachingSettings"/> settings</param>
    /// <param name="configure">Further configuration as needed if caching is enabled</param>
    public static void AddCaching(this IServiceCollection services, IConfiguration configuration, Action<IServiceCollection, IConfiguration> configure)
    {
        var config = configuration.GetSection(CachingSettings.SectionName).Get<CachingSettings>();

        if (config is not null && config.EnableUseCaseCaching)
        {
            services.Configure<CachingSettings>(configuration.GetSection(CachingSettings.SectionName));
            services.AddSingleton<InMemoryCache>();
            configure(services, configuration);
        }
        else
        {
            services.AddSingleton<ICache, PassThroughCache>();
        }
    }

    /// <summary>
    /// Adds a <see cref="NestedCache{TOuterCache, TInnerCache}"/> implementation of <see cref="ICache"/>, removing all
    /// previous implementations
    /// </summary>
    /// <typeparam name="TOuterCache">The outer cache</typeparam>
    /// <typeparam name="TInnerCache">The inner cache</typeparam>
    /// <param name="services">The service collection</param>
    public static void AddNestedCache<TOuterCache, TInnerCache>(this IServiceCollection services)
        where TOuterCache : class, ICache
        where TInnerCache : class, ICache
    {
        services.RemoveAll<ICache>();
        services.AddSingleton<ICache, NestedCache<TOuterCache, TInnerCache>>();
    }

    /// <summary>
    /// Adds a specific implementation of <see cref="IUseCase{TRequest, TResponse}"/>, wrapping it with <see cref="CachedUseCase{TRequest, TResponse}"/> if
    /// use case caching is enabled in <see cref="CachingSettings"/> defined within <paramref name="configuration"/>.
    /// </summary>
    /// <typeparam name="TImplementation">Use case implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration to find <see cref="CachingSettings"/> settings</param>
    public static void AddUseCase<TImplementation>(this IServiceCollection services, IConfiguration configuration)
        where TImplementation : class
        => services.AddUseCase(typeof(TImplementation), configuration);

    private static void AddUseCase(this IServiceCollection services, Type implementationType, IConfiguration configuration)
    {
        var config = configuration.GetSection(CachingSettings.SectionName).Get<CachingSettings>();

        var interfaceType = GetInterfaceType(implementationType);

        if (config is null || !config.EnableUseCaseCaching)
        {
            services.AddSingleton(interfaceType, implementationType);
            return;
        }

        services.AddKeyedSingleton(interfaceType, ServiceKeys.RealImplementation, implementationType);
        services.AddSingleton(interfaceType, sp =>
            sp.GetCachedUseCase(interfaceType, ServiceKeys.RealImplementation));
    }

    /// <summary>
    /// Adds a specific keyed implementation of <see cref="IUseCase{TRequest, TResponse}"/>, wrapping it with <see cref="CachedUseCase{TRequest, TResponse}"/> if
    /// use case caching is enabled in <see cref="CachingSettings"/> defined within <paramref name="configuration"/>.
    /// </summary>
    /// <typeparam name="TImplementation">Use case implementation type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="serviceKey">The service key</param>
    /// <param name="configuration">Configuration to find <see cref="CachingSettings"/> settings</param>
    public static void AddKeyedUseCase<TImplementation>(this IServiceCollection services, string serviceKey, IConfiguration configuration)
        where TImplementation : class
        => services.AddKeyedUseCase(serviceKey, typeof(TImplementation), configuration);

    private static void AddKeyedUseCase(this IServiceCollection services, string serviceKey, Type implementationType, IConfiguration configuration)
    {
        var config = configuration.GetSection(CachingSettings.SectionName).Get<CachingSettings>();

        var interfaceType = GetInterfaceType(implementationType);

        if (config is null || !config.EnableUseCaseCaching)
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
