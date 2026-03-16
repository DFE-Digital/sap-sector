using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;

namespace SAPSec.Infrastructure.LuceneSearch;

public class StartupIndexBuilder(
    ILogger<StartupIndexBuilder> logger,
    LuceneIndexWriter writer,
    IEstablishmentRepository establishmentRepository)
    : BackgroundService
{
    private const int RetryIntervalSeconds = 10;
    private long _indexBuiltSuccessfully = 0;

    private bool IndexBuiltSuccessfully
    {
        get => Interlocked.Read(ref _indexBuiltSuccessfully) == 1;
        set => Interlocked.Exchange(ref _indexBuiltSuccessfully, Convert.ToInt64(value));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await TryBuildIndex(cancellationToken);

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(RetryIntervalSeconds));

            while (!IndexBuiltSuccessfully
                && await timer.WaitForNextTickAsync(cancellationToken))
            {
                await TryBuildIndex(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation cancelled.");
        }
    }

    private async Task TryBuildIndex(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Reading establishment data at startup...");

            cancellationToken.ThrowIfCancellationRequested();

            var schools = await establishmentRepository.GetAllEstablishmentsAsync();

            if (!schools.Any())
            {
                logger.LogInformation("No establishment data was found, waiting for database to be fully generated.");

                return;
            }

            logger.LogInformation("Establishment Data retrieved successfully.");

            logger.LogInformation("Building Lucene index at startup...");

            writer.BuildIndex(schools, cancellationToken);
            IndexBuiltSuccessfully = true;

            logger.LogInformation("Lucene index built successfully.");
        }
        catch (Exception ex)
        {
            // If building the index fails, log and swallow the exception,
            // otherwise the application will not complete startup and remain
            // in Service Unavailable state
            logger.LogError(ex, "Reading establishment data failed.");
        }
    }
}
