using SAPSec.Core.Model;
using SAPSec.Core.Rules;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Tests.Rules;

public class ResourcedProvisionRuleTests
{
    private readonly ResourcedProvisionRule _sut = new();

    [Theory]
    [InlineData("Resourced provision")]
    [InlineData("Resourced provision and SEN unit")]
    public void Evaluate_HasResourcedProvision_ReturnsExpected(string? resourceProvision)
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
    [InlineData("Not applicable")]
    [InlineData("SEN unit")]
    public void Evaluate_NoResourcedProvision_ReturnsExpected(string? resourceProvision)
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
    [InlineData(null)]
    public void Evaluate_NotAvailable_ReturnsExpected(string? resourceProvision)
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvisionName = resourceProvision
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }
}
