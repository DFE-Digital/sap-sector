using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Interfaces.Services.Lucene;
using SAPSec.Core.Services;
using SAPSec.Core.Services.Lucene;

namespace SAPSec.Core;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddCoreDependencies(this IServiceCollection services)
    {
        services.AddSingleton<LuceneIndexContext>();
        services.AddSingleton<ILuceneIndexWriter, LuceneIndexWriter>();
        services.AddSingleton<ILuceneIndexReader, LuceneIndexReader>();
        services.AddSingleton<ILuceneHighlighter, LuceneHighlighter>();
        services.AddSingleton<ILuceneSynonymMapBuilder, LuceneSynonymMapBuilder>();
        services.AddSingleton<ILuceneTokeniser, LuceneTokeniser>();

        services.AddSingleton<ISearchService, SearchService>();

        services.AddHostedService<StartupIndexBuilder>();
    }
}
