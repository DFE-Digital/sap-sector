using SAPSec.Test.Integration.Setup;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration;

[Collection("JsonRepositoryIntegrationTestsCollection")]
public abstract class JsonRepositoryIntegrationTests(
    JsonRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : IAsyncLifetime
{
    protected JsonRepositoryIntegrationTestFixture Fixture => fixture;
    protected ITestOutputHelper OutputHelper => outputHelper;

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;
}
