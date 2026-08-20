using SAPSec.Test.EndToEnd.Setup;
using Xunit;

namespace SAPSec.Test.Accessibility.Setup;

[CollectionDefinition("AccessibilityTestsCollection")]
public class AccessibilityTestsCollection : ICollectionFixture<AccessibilityTestsFixture>
{
    // This class is intentionally empty. 
    // It's used solely to apply the [CollectionDefinition] attribute.
}