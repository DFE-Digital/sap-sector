using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.School.Search;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Infrastructure.LuceneSearch;

[ExcludeFromCodeCoverage]
public static class LuceneExtensions
{
    public static void AddLuceneDependencies(this IServiceCollection services)
    {
        services.AddSingleton<LuceneIndexContext>();
        services.AddSingleton<LuceneIndexWriter>();
        services.AddSingleton<LuceneShoolSearchIndexReader>();
        services.AddSingleton<LuceneHighlighter>();
        services.AddSingleton<LuceneSynonymMapBuilder>();
        services.AddSingleton<LuceneTokeniser>();

        services.AddSingleton<ISchoolSearchIndexReader, LuceneShoolSearchIndexReader>();
        services.AddHostedService<StartupIndexBuilder>();
    }
}
