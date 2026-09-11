using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Setup;

[Collection("InMemoryRepositoryIntegrationTestsCollection")]
public abstract class InMemoryRepositoryIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : IAsyncLifetime
{
    protected InMemoryRepositoryIntegrationTestFixture Fixture => fixture;
    protected ITestOutputHelper OutputHelper => outputHelper;

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync()
    {
        Fixture.ClearDownRepositories();

        return Task.CompletedTask;
    }
}
