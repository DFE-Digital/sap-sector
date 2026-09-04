using SAPSec.Web.Formatters;
using SAPSec.Web.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DependenciesExtensions
{
    public static void AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<IRequestSchoolAccessor, RequestSchoolAccessor>();

        // Formatters
        services.AddSingleton<ICharacteristicsComparisonFormatter, CharacteristicsComparisonFormatter>();
        services.AddSingleton<IPrimaryCharacteristicsComparisonFormatter, PrimaryCharacteristicsComparisonFormatter>();
    }
}
