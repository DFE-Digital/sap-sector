using Xunit;

namespace SAPSec.Test.EndToEnd.Setup;

[CollectionDefinition("EndToEndTestsCollection")]
public class EndToEndTestsCollection : ICollectionFixture<EndToEndTestsFixture>
{
    // This class is intentionally empty. 
    // It's used solely to apply the [CollectionDefinition] attribute.
}