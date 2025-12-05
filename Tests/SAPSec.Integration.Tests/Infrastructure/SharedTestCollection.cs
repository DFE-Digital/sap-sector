namespace SAPSec.Integration.Tests.Infrastructure;

[CollectionDefinition("IntegrationTestsCollection")]
public class SharedTestCollection : ICollectionFixture<WebApplicationSetupFixture>
{
    // This class is intentionally empty. 
    // It's used solely to apply the [CollectionDefinition] attribute.
}