using Xunit;

namespace SAPSec.UI.Tests.Infrastructure;

[CollectionDefinition("UITestsCollection")]
public class SharedTestCollection : ICollectionFixture<WebApplicationSetupFixture>
{
    // This class is intentionally empty. 
    // It's used solely to apply the [CollectionDefinition] attribute.
}