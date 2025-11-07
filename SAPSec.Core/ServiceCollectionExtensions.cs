using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services.Lucene;

namespace SAPSec.Core;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddCoreDependencies(this IServiceCollection services)
    {
        services.AddSingleton<IAbbreviationExpander, AbbreviationExpander>();
        services.AddSingleton<IQueryTokeniser, QueryTokeniser>();

        // Lucene shared context and services
        services.AddSingleton<LuceneIndexContext>();
        services.AddSingleton<LuceneSearchIndexWriterService>();
        services.AddSingleton<ISearchService, LuceneSearchIndexReaderService>();

        services.AddHostedService<StartupIndexBuilder>();
    }
}
