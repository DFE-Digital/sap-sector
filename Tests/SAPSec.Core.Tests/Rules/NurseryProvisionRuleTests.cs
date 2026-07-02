using SAPSec.Core.Model;
using SAPSec.Core.Rules;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Tests.Rules;

public class NurseryProvisionRuleTests
{
    private readonly NurseryProvisionRule _sut = new();

    [Theory]
    [InlineData("Has Nursery Classes", true)]
    public void Evaluate_HasNurseryProvision_ReturnsExpected(string nurseryProvisionName, bool expected)
    {
        // Arrange
        var establishment = new Establishment
        {
            NurseryProvisionName = nurseryProvisionName
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("No Nursery Classes", false)]
    public void Evaluate_NoNurseryProvision_ReturnsExpected(string? nurseryProvisionName, bool expected)
    {
        // Arrange
        var establishment = new Establishment
        {
            NurseryProvisionName = nurseryProvisionName
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    public void Evaluate_Blanks_ReturnsNotAvailable(string? nurseryProvisionName)
    {
        // Arrange
        var establishment = new Establishment
        {
            NurseryProvisionName = nurseryProvisionName
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }
}

