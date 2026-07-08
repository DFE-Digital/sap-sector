using SAPSec.Test.Integration.Setup;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration;

[Collection("InMemoryStoreIntegrationTestsCollection")]
public abstract class InMemoryStoreIntegrationTests(
    InMemoryStoreIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : IAsyncLifetime
{
    protected InMemoryStoreIntegrationTestFixture Fixture => fixture;
    protected ITestOutputHelper OutputHelper => outputHelper;

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;
}
