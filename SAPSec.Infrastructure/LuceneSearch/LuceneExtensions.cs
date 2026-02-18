using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace SAPSec.Infrastructure.LuceneSearch;

[ExcludeFromCodeCoverage]
public static class LuceneExtensions
{
    public static void AddLuceneDependencies(this IServiceCollection services, bool enableStartupIndexBuilder = true)
    {
        services.AddSingleton<LuceneIndexContext>();
        services.AddSingleton<LuceneIndexWriter>();
        services.AddSingleton<LuceneShoolSearchIndexReader>();
        services.AddSingleton<LuceneHighlighter>();
        services.AddSingleton<LuceneSynonymMapBuilder>();
        services.AddSingleton<LuceneTokeniser>();
        if (enableStartupIndexBuilder)
        {
            services.AddHostedService<StartupIndexBuilder>();
        }
    }
}
