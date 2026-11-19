using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Caching;

namespace SAPSec.Test.Integration.Setup;

public class JsonRepositoryIntegrationTestFixture : IntegrationTestFixture
{
    public ICache Cache => _factory.Services.GetRequiredService<ICache>();

    protected override IntegrationTestsWebApplicationFactory CreateWebApplicationFactory() =>
        new JsonRepositoryIntegrationTestsWebApplicationFactory();

    public override async Task ClearDown()
    {
        await Cache.Clear();
    }
}