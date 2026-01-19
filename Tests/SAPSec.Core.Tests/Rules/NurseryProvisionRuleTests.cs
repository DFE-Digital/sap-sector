using SAPSec.Core.Model;
using SAPSec.Core.Rules;

namespace SAPSec.Core.Tests.Rules;

public class NurseryProvisionRuleTests
{
    private readonly NurseryProvisionRule _sut = new();

    [Theory]
    [InlineData("Nursery", true)]
    [InlineData("nursery school", true)]
    [InlineData("Primary", false)]
    [InlineData("Secondary", false)]
    [InlineData("16 plus", false)]
    [InlineData("Post-16", false)]
    [InlineData("16-19", false)]
    public void Evaluate_KnownPhases_ReturnsExpected(string phase, bool expected)
    {
        // Arrange
        var establishment = new Establishment
        {
            PhaseOfEducationName = phase
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Evaluate_NoPhase_ReturnsNotAvailable(string? phase)
    {
        // Arrange
        var establishment = new Establishment
        {
            PhaseOfEducationName = phase
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailability.NotAvailable);
    }

    [Fact]
    public void Evaluate_AllThrough_ReturnsNotAvailable()
    {
        // Arrange
        var establishment = new Establishment
        {
            PhaseOfEducationName = "All-through"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailability.NotAvailable);
    }
}

