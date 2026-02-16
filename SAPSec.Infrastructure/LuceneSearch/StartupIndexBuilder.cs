using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Infrastructure.LuceneSearch;

public class StartupIndexBuilder(ILogger<StartupIndexBuilder> logger, LuceneIndexWriter writer, IEstablishmentService establishmentService) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Reading establishment data from Postgres at startup...");

        var schools = establishmentService.GetAllEstablishments();

        logger.LogInformation("Establishment Data retrieved successfully");

        logger.LogInformation("Building Lucene index at startup...");

        writer.BuildIndex(schools);

        logger.LogInformation("Lucene index built successfully");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
