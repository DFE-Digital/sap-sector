using SAPSec.Core.Model;
using SAPSec.Core.Rules;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Tests.Rules;

public class SenUnitRuleTests
{
    private readonly SenUnitRule _sut = new();


    [Theory]
    [InlineData("Resourced provision and SEN unit")]
    [InlineData("SEN unit")]
    public void Evaluate_HasSENUnit_ReturnsExpected(string resourceProvision)
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvisionName = resourceProvision
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Theory]
    [InlineData("Resourced provision")]
    [InlineData("Not applicable")]
    public void Evaluate_NoSENUnit_ReturnsExpected(string resourceProvision)
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvisionName = resourceProvision
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    public void Evaluate_NotAvailable_ReturnsExpected(string resourceProvision)
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvisionName = resourceProvision
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }
}
