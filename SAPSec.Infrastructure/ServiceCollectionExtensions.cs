using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Infrastructure.Interfaces;
using SAPSec.Infrastructure.Repositories;

namespace SAPSec.Infrastructure;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddInfrastructureDependencies(this IServiceCollection services, string? csvPath = null)
    {
        services.AddSingleton<ISchoolRepository>(_ => new SchoolCsvFileRepository(csvPath ?? string.Empty));
    }
}
