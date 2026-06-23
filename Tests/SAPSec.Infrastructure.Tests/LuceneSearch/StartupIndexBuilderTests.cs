using Microsoft.Extensions.Logging;
using SAPSec.Core.Constants;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Test.Common;
using SAPSec.Test.Common.Repositories.InMemory;
using Xunit.Abstractions;

namespace SAPSec.Infrastructure.Tests.LuceneSearch;

public class StartupIndexBuilderTests(ITestOutputHelper output)
{
    private const int TimeBetweenAttemptsMilliseconds = 100;
    private const int IndexBuilderDataReadIntervalMilliseconds = 50;
    private const int NumberOfAttemptsUntilTestTimeout = 20;
    private const int PopulateEstablishmentDataOnAttempt = 2;

    private TestOutputLogger<StartupIndexBuilder> _logger = new(output);
    private InMemoryEstablishmentRepository _establishmentRepo = new();
    private TestFeatureFlagService _featureFlagService = new();

    [Fact]
    public async Task StartAsync_PopulatesIndexWithEstablishments_And_Completes()
    {
        _logger.LogInformation("Start test");
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", EstablishmentName = "Test School 1", PhaseOfEducationName = "Secondary", EstablishmentStatusName = "Open" },
            new() { URN = "100002", EstablishmentName = "Test School 2", PhaseOfEducationName = "Secondary", EstablishmentStatusName = "Open" }
        );

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        var sut = new StartupIndexBuilder(_logger, writer, _establishmentRepo, _featureFlagService,
            IndexBuilderDataReadIntervalMilliseconds);

        _logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        _logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            _logger.LogInformation($"Attempt {attempts}");
        }

        _logger.LogInformation("Attempts complete");

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
        _logger.LogInformation("Start test");
        _establishmentRepo.SetupEstablishments();

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        var sut = new StartupIndexBuilder(_logger, writer, _establishmentRepo, _featureFlagService,
            IndexBuilderDataReadIntervalMilliseconds);

        _logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        _logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            _logger.LogInformation($"Attempt {attempts}");
        }

        _logger.LogInformation("Attempts complete");

        await sut.StopAsync(CancellationToken.None);
        sut.IndexBuiltSuccessfully.Should().Be(false);
    }

    [Fact]
    public async Task StartAsync_WhenEstablishmentsInitiallyEmpty_WaitsUntilEstablishmentsPopulatedAndThenBuildsIndex()
    {
        _establishmentRepo.SetupEstablishments();

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        var sut = new StartupIndexBuilder(_logger, writer, _establishmentRepo, _featureFlagService,
            IndexBuilderDataReadIntervalMilliseconds);

        _logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        _logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            _logger.LogInformation($"Attempt {attempts}");
            if (attempts == PopulateEstablishmentDataOnAttempt)
            {
                var establishments = await _establishmentRepo.GetAllEstablishmentsAsync();
                if (!establishments.Any())
                {
                    _establishmentRepo.SetupEstablishments(
                        new() { URN = "100001", EstablishmentName = "Test School 1", PhaseOfEducationName = "Secondary", EstablishmentStatusName = "Open" },
                        new() { URN = "100002", EstablishmentName = "Test School 2", PhaseOfEducationName = "Secondary", EstablishmentStatusName = "Open" }
                    );
                }
            }
        }

        _logger.LogInformation("Attempts complete");

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
    public async Task StartAsync_IfPrimarySchoolsEnabled_ShouldIndexPrimarySchools()
    {
        _logger.LogInformation("Start test");
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", EstablishmentName = "Test Primary 1", PhaseOfEducationName = "Primary", PhaseOfEducationId = "2", EstablishmentStatusName = "Open" },
            new() { URN = "100002", EstablishmentName = "Test Secondary 2", PhaseOfEducationName = "Secondary", PhaseOfEducationId = "4", EstablishmentStatusName = "Open" },
            new() { URN = "100003", EstablishmentName = "Test Nursery 3", PhaseOfEducationName = "Nursery", PhaseOfEducationId = "1", EstablishmentStatusName = "Open" },
            new() { URN = "100004", EstablishmentName = "Test All Through 4", PhaseOfEducationName = "All-through", PhaseOfEducationId = "7", EstablishmentStatusName = "Open" }
        );

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        _featureFlagService.SetFeatureEnabled(FeatureFlags.EnablePrimarySchools, true);
        var sut = new StartupIndexBuilder(_logger, writer, _establishmentRepo, _featureFlagService,
            IndexBuilderDataReadIntervalMilliseconds);

        _logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        _logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            _logger.LogInformation($"Attempt {attempts}");
        }

        _logger.LogInformation("Attempts complete");

        await sut.StopAsync(CancellationToken.None);
        sut.IndexBuiltSuccessfully.Should().Be(true);

        var reader = new LuceneShoolSearchIndexReader(ctx, new LuceneTokeniser(ctx), new LuceneHighlighter());

        var results = await reader.SearchAsync("test");
        results.Should().BeEquivalentTo([
            (100001, "*Test* Primary 1"),
            (100002, "*Test* Secondary 2"),
            (100004, "*Test* All Through 4")
        ]);
    }

    [Fact]
    public async Task StartAsync_IfPrimarySchoolsDisabled_ShouldNotIndexPrimaryOrAllThroughSchools()
    {
        _logger.LogInformation("Start test");
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", EstablishmentName = "Test Primary 1", PhaseOfEducationName = "Primary", PhaseOfEducationId = "2", EstablishmentStatusName = "Open" },
            new() { URN = "100002", EstablishmentName = "Test Secondary 2", PhaseOfEducationName = "Secondary", PhaseOfEducationId = "4", EstablishmentStatusName = "Open" },
            new() { URN = "100003", EstablishmentName = "Test Nursery 3", PhaseOfEducationName = "Nursery", PhaseOfEducationId = "1", EstablishmentStatusName = "Open" },
            new() { URN = "100004", EstablishmentName = "Test All Through 4", PhaseOfEducationName = "All-through", PhaseOfEducationId = "7", EstablishmentStatusName = "Open" }
        );

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        _featureFlagService.SetFeatureEnabled(FeatureFlags.EnablePrimarySchools, false);
        var sut = new StartupIndexBuilder(_logger, writer, _establishmentRepo, _featureFlagService,
            IndexBuilderDataReadIntervalMilliseconds);

        _logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        _logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            _logger.LogInformation($"Attempt {attempts}");
        }

        _logger.LogInformation("Attempts complete");

        await sut.StopAsync(CancellationToken.None);
        sut.IndexBuiltSuccessfully.Should().Be(true);

        var reader = new LuceneShoolSearchIndexReader(ctx, new LuceneTokeniser(ctx), new LuceneHighlighter());

        var results = await reader.SearchAsync("test");
        results.Should().BeEquivalentTo([
            (100002, "*Test* Secondary 2")
        ]);
    }

    [Fact]
    public async Task StartAsync_ShouldNotIndexClosedSchools()
    {
        _logger.LogInformation("Start test");
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", EstablishmentName = "Test Primary 1", PhaseOfEducationName = "Primary", PhaseOfEducationId = "2", EstablishmentStatusId = "1" },
            new() { URN = "100002", EstablishmentName = "Test Secondary 2", PhaseOfEducationName = "Secondary", PhaseOfEducationId = "4", EstablishmentStatusId = "2" },
            new() { URN = "100003", EstablishmentName = "Test Nursery 3", PhaseOfEducationName = "Nursery", PhaseOfEducationId = "1", EstablishmentStatusName = "  Open " },
            new() { URN = "100004", EstablishmentName = "Test All Through 4", PhaseOfEducationName = "All-through", PhaseOfEducationId = "7", EstablishmentStatusName = "  Closed " }
        );

        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        _featureFlagService.SetFeatureEnabled(FeatureFlags.EnablePrimarySchools, true);
        var sut = new StartupIndexBuilder(_logger, writer, _establishmentRepo, _featureFlagService,
            IndexBuilderDataReadIntervalMilliseconds);

        _logger.LogInformation("Start builder");
        await Task.Run(async () => await sut.StartAsync(CancellationToken.None));

        _logger.LogInformation("Start timer");
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(TimeBetweenAttemptsMilliseconds));
        var attempts = 0;

        while (!sut.IndexBuiltSuccessfully
            && ++attempts <= NumberOfAttemptsUntilTestTimeout
            && await timer.WaitForNextTickAsync(CancellationToken.None))
        {
            _logger.LogInformation($"Attempt {attempts}");
        }

        _logger.LogInformation("Attempts complete");

        await sut.StopAsync(CancellationToken.None);
        sut.IndexBuiltSuccessfully.Should().Be(true);

        var reader = new LuceneShoolSearchIndexReader(ctx, new LuceneTokeniser(ctx), new LuceneHighlighter());

        var results = await reader.SearchAsync("test");
        results.Should().BeEquivalentTo([
            (100001, "*Test* Primary 1"),
        ]);
    }
}
