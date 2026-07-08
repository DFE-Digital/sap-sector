namespace SAPSec.Test.Integration.Setup;

public class JsonRepositoryIntegrationTestFixture : IntegrationTestFixture
{
    protected override IntegrationTestsWebApplicationFactory CreateWebApplicationFactory() =>
        new JsonRepositoryIntegrationTestsWebApplicationFactory();
}