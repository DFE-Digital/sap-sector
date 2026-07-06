namespace SAPSec.Test.Integration.Setup;

public class JsonStoreIntegrationTestFixture : IntegrationTestFixture
{
    protected override IntegrationTestsWebApplicationFactory CreateWebApplicationFactory() =>
        new JsonStoreIntegrationTestsWebApplicationFactory();
}