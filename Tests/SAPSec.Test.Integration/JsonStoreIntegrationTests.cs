using SAPSec.Test.Integration.Setup;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration;

[Collection("JsonStoreIntegrationTestsCollection")]
public abstract class JsonStoreIntegrationTests(
    JsonStoreIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : IAsyncLifetime
{
    protected JsonStoreIntegrationTestFixture Fixture => fixture;
    protected ITestOutputHelper OutputHelper => outputHelper;

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;
}
