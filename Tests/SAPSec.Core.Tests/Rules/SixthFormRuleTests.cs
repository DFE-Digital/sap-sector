using SAPSec.Core.Model;
using SAPSec.Core.Rules;

namespace SAPSec.Core.Tests.Rules;

public class SixthFormRuleTests
{
    private readonly SixthFormRule _sut = new();

    [Fact]
    public void Evaluate_Code1_ReturnsTrue()
    {
        // Arrange
        var establishment = new Establishment { OfficialSixthFormId = "1" };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_Code2_ReturnsFalse()
    {
        // Arrange
        var establishment = new Establishment { OfficialSixthFormId = "2" };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_Code0_ReturnsNotApplicable()
    {
        // Arrange
        var establishment = new Establishment { OfficialSixthFormId = "0" };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailability.NotApplicable);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    public void Evaluate_InvalidValue_ReturnsNotAvailable(string? value)
    {
        // Arrange
        var establishment = new Establishment { OfficialSixthFormId = value };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailability.NotAvailable);
    }
}
