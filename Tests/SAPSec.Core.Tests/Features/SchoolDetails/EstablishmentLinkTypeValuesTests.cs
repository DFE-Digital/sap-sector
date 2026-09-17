using SAPSec.Core.Features.SchoolDetails;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class EstablishmentLinkTypeValuesTests
{
    [Theory]
    [InlineData("Successor")]
    [InlineData("Successor - Split School")]
    [InlineData("Successor - amalgamated")]
    [InlineData("successor")]
    [InlineData("  Successor  ")]
    public void IsSuccessor_ReturnsTrue_ForSuccessorLinkTypes(string linkType)
    {
        EstablishmentLinkTypeValues.IsSuccessor(linkType).Should().BeTrue();
    }

    [Theory]
    [InlineData("Predecessor")]
    [InlineData("Predecessor - amalgamated")]
    [InlineData("Sixth Form Centre Link")]
    [InlineData("Result of Amalgamation")]
    [InlineData("Other")]
    [InlineData(null)]
    [InlineData("")]
    public void IsSuccessor_ReturnsFalse_ForNonSuccessorLinkTypes(string? linkType)
    {
        EstablishmentLinkTypeValues.IsSuccessor(linkType).Should().BeFalse();
    }

    [Theory]
    [InlineData("Predecessor")]
    [InlineData("Predecessor - Split School")]
    [InlineData("Predecessor - merged")]
    public void IsPredecessor_ReturnsTrue_ForPredecessorLinkTypes(string linkType)
    {
        EstablishmentLinkTypeValues.IsPredecessor(linkType).Should().BeTrue();
    }

    [Theory]
    [InlineData("Successor")]
    [InlineData("Other")]
    [InlineData(null)]
    public void IsPredecessor_ReturnsFalse_ForNonPredecessorLinkTypes(string? linkType)
    {
        EstablishmentLinkTypeValues.IsPredecessor(linkType).Should().BeFalse();
    }
}
