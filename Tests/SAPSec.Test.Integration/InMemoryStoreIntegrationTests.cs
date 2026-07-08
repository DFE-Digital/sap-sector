using SAPSec.Test.Integration.Setup;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration;

[Collection("InMemoryRepositoryIntegrationTestsCollection")]
public abstract class InMemoryRepositoryIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : IAsyncLifetime
{
    protected InMemoryRepositoryIntegrationTestFixture Fixture => fixture;
    protected ITestOutputHelper OutputHelper => outputHelper;

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;
}
