using SAPSec.Core.Model;
using SAPSec.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Rules;

public class ResourcedProvisionRuleTests
{
    private readonly ResourcedProvisionRule _sut = new();

    [Fact]
    public void Evaluate_ContainsResourcedProvision_ReturnsTrue()
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvisionName = "Has resourced provision"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Not applicable")]
    [InlineData("None")]
    public void Evaluate_NoProvision_ReturnsFalse(string? provision)
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvisionName = provision
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_OnlySenUnit_ReturnsFalse()
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvisionName = "Has SEN unit"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeFalse();
    }
}
