using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Infrastructure.LuceneSearch;

public class StartupIndexBuilder(ILogger<StartupIndexBuilder> logger, LuceneIndexWriter writer, IEstablishmentService establishmentService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("reading Establishment Data From CSV at startup...");

        var schools = await establishmentService.GetAllEstablishmentsAsync();

        logger.LogInformation("Establishment Data retrieved successfully");

        logger.LogInformation("Building Lucene index at startup...");

        writer.BuildIndex(schools);

        logger.LogInformation("Lucene index built successfully");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
