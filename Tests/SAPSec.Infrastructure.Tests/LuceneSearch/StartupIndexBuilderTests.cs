using Microsoft.Extensions.Logging;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Test.Common;
using SAPSec.Test.Common.Repositories.InMemory;
using Xunit.Abstractions;

namespace SAPSec.Infrastructure.Tests.LuceneSearch;

public class StartupIndexBuilderTests(ITestOutputHelper output)
{
    private const int TimeBetweenAttemptsMilliseconds = 100;
    private const int IndexBuilderDataReadIntervalMilliseconds = 50;
    private const int NumberOfAttemptsUntilTestTimeout = 3;
    private const int PopulateEstablishmentDataOnAttempt = 2;

    private TestOutputLogger<StartupIndexBuilder> logger = new(output);
    private InMemoryEstablishmentRepository establishmentRepo = new();
    private InMemorySimilarSchoolsSecondaryRepository similarSchoolsRepo = new();

    [Fact]
    public async Task StartAsync_PopulatesIndexWithEstablishments_And_Completes()
    {
        // Arrange
        logger.LogInformation("Start test");
        establishmentRepo.SetupEstablishments(
            new() { URN = "100001", EstablishmentName = "Test School 1" },
            new() { URN = "100002", EstablishmentName = "Test School 2" }
        );
        similarSchoolsRepo.SetupValues(
            new() { URN = "100001" },
            new() { URN = "100002" }
        );

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        var sut = new StartupIndexBuilder(logger, writer, establishmentRepo, similarSchoolsRepo,
            IndexBuilderDataReadIntervalMilliseconds);

        // Act
        logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            logger.LogInformation($"Attempt {attempts}");
        }

        logger.LogInformation("Attempts complete");

        // Assert
        await sut.StopAsync(CancellationToken.None);
        sut.IndexBuiltSuccessfully.Should().Be(true);

        var reader = new LuceneShoolSearchIndexReader(ctx, new LuceneTokeniser(ctx), new LuceneHighlighter());

        var results = await reader.SearchAsync("test");
        results.Should().BeEquivalentTo([
            (100001, "*Test* School 1"),
            (100002, "*Test* School 2")
        ]);
    }

    [Fact]
    public async Task StartAsync_WhenEstablishmentsEmpty_DoesNotCompleteAndKeepsWaiting()
    {
        // Arrange
        logger.LogInformation("Start test");
        establishmentRepo.SetupEstablishments();
        similarSchoolsRepo.SetupValues();

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        var sut = new StartupIndexBuilder(logger, writer, establishmentRepo, similarSchoolsRepo,
            IndexBuilderDataReadIntervalMilliseconds);

        // Act
        logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0; ;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            logger.LogInformation($"Attempt {attempts}");
        }

        logger.LogInformation("Attempts complete");

        // Assert
        await sut.StopAsync(CancellationToken.None);
        sut.IndexBuiltSuccessfully.Should().Be(false);
    }

    [Fact]
    public async Task StartAsync_WhenEstablishmentsInitiallyEmpty_WaitsUntilEstablishmentsPopulatedAndThenBuildsIndex()
    {
        // Arrange
        establishmentRepo.SetupEstablishments();
        similarSchoolsRepo.SetupValues();

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        var sut = new StartupIndexBuilder(logger, writer, establishmentRepo, similarSchoolsRepo,
            IndexBuilderDataReadIntervalMilliseconds);

        // Act
        logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0; ;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            logger.LogInformation($"Attempt {attempts}");
            if (attempts == PopulateEstablishmentDataOnAttempt)
            {
                var establishments = await establishmentRepo.GetAllEstablishmentsAsync();
                if (!establishments.Any())
                {
                    establishmentRepo.SetupEstablishments(
                        new() { URN = "100001", EstablishmentName = "Test School 1" },
                        new() { URN = "100002", EstablishmentName = "Test School 2" }
                    );
                    similarSchoolsRepo.SetupValues(
                        new() { URN = "100001" },
                        new() { URN = "100002" }
                    );
                }
            }
        }

        logger.LogInformation("Attempts complete");

        // Assert
        await sut.StopAsync(CancellationToken.None);
        sut.IndexBuiltSuccessfully.Should().Be(true);

        var reader = new LuceneShoolSearchIndexReader(ctx, new LuceneTokeniser(ctx), new LuceneHighlighter());

        var results = await reader.SearchAsync("test");
        results.Should().BeEquivalentTo([
            (100001, "*Test* School 1"),
            (100002, "*Test* School 2")
        ]);
    }

    [Fact]
    public async Task StartAsync_ShouldFilterOutEstablishmentsNotInSimilarSchoolsDataSet()
    {
        // Arrange
        logger.LogInformation("Start test");
        establishmentRepo.SetupEstablishments(
            new() { URN = "100001", EstablishmentName = "Test School 1" },
            new() { URN = "100002", EstablishmentName = "Test School 2" },
            new() { URN = "100003", EstablishmentName = "Test School 3" },
            new() { URN = "100004", EstablishmentName = "Test School 4" }
        );
        similarSchoolsRepo.SetupValues(
            new() { URN = "100001" },
            new() { URN = "100002" }
        );

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        var sut = new StartupIndexBuilder(logger, writer, establishmentRepo, similarSchoolsRepo,
            IndexBuilderDataReadIntervalMilliseconds);

        // Act
        logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            logger.LogInformation($"Attempt {attempts}");
        }

        logger.LogInformation("Attempts complete");

        // Assert
        await sut.StopAsync(CancellationToken.None);
        sut.IndexBuiltSuccessfully.Should().Be(true);

        var reader = new LuceneShoolSearchIndexReader(ctx, new LuceneTokeniser(ctx), new LuceneHighlighter());

        var results = await reader.SearchAsync("test");
        results.Should().BeEquivalentTo([
            (100001, "*Test* School 1"),
            (100002, "*Test* School 2")
        ]);
    }
}
