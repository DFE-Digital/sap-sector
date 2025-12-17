using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services;
using SAPSec.Infrastructure.LuceneSearch.Implementation;
using SAPSec.Infrastructure.LuceneSearch.Interfaces;

namespace SAPSec.Infrastructure.LuceneSearch;

[ExcludeFromCodeCoverage]
public static class LuceneExtensions
{
    public static void AddLuceneDependencies(this IServiceCollection services)
    {
        services.AddSingleton<LuceneIndexContext>();
        services.AddSingleton<ILuceneIndexWriter, LuceneIndexWriter>();
        services.AddSingleton<ILuceneIndexReader, LuceneIndexReader>();
        services.AddSingleton<ILuceneHighlighter, LuceneHighlighter>();
        services.AddSingleton<ILuceneSynonymMapBuilder, LuceneSynonymMapBuilder>();
        services.AddSingleton<ILuceneTokeniser, LuceneTokeniser>();
        services.AddHostedService<StartupIndexBuilder>();
    }
}
