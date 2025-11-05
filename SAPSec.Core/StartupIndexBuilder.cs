using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Services.Lucene;

namespace SAPSec.Core;

public class StartupIndexBuilder(ILogger<StartupIndexBuilder> logger, LuceneSearchIndexWriterService writer) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Building Lucene index at startup...");
        writer.BuildIndex();
        logger.LogInformation("Lucene index built successfully");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
