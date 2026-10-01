using SAPSec.Core.Features.SchoolDetails;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class SuccessorRelationshipTypeValuesTests
{
    [Theory]
    [InlineData("Successor")]
    [InlineData("successor")]
    [InlineData("  Successor  ")]
    [InlineData("Successor - amalgamated")]
    [InlineData("Result of Amalgamation")]
    [InlineData("result of amalgamation")]
    public void IsMapped_ReturnsTrue_ForCoveredRelationshipTypes(string linkType)
    {
        SuccessorRelationshipTypeValues.IsMapped(linkType).Should().BeTrue();
    }

    [Theory]
    [InlineData("Successor - Split School")]
    [InlineData("Predecessor")]
    [InlineData("Predecessor - amalgamated")]
    [InlineData("Sixth Form Centre Link")]
    [InlineData("Other")]
    [InlineData(null)]
    [InlineData("")]
    public void IsMapped_ReturnsFalse_ForUncoveredRelationshipTypes(string? linkType)
    {
        SuccessorRelationshipTypeValues.IsMapped(linkType).Should().BeFalse();
    }
}
