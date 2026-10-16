using SAPSec.Core.Features.SchoolDetails;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class PredecessorRelationshipTypeValuesTests
{
    [Theory]
    [InlineData("Predecessor")]
    [InlineData("predecessor")]
    [InlineData("  Predecessor  ")]
    [InlineData("Predecessor - amalgamated")]
    [InlineData("Predecessor - merged")]
    public void IsMapped_ReturnsTrue_ForCoveredRelationshipTypes(string linkType)
    {
        PredecessorRelationshipTypeValues.IsMapped(linkType).Should().BeTrue();
    }

    [Theory]
    [InlineData("Predecessor - Split School")]
    [InlineData("Successor")]
    [InlineData("Successor - amalgamated")]
    [InlineData("Result of Amalgamation")]
    [InlineData("Sixth Form Centre Link")]
    [InlineData("Other")]
    [InlineData(null)]
    [InlineData("")]
    public void IsMapped_ReturnsFalse_ForUncoveredRelationshipTypes(string? linkType)
    {
        PredecessorRelationshipTypeValues.IsMapped(linkType).Should().BeFalse();
    }
}
