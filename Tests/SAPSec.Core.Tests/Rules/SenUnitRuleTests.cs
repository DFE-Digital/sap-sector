using SAPSec.Core.Model;
using SAPSec.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Rules;


public class SenUnitRuleTests
{
    private readonly SenUnitRule _sut = new();

    [Fact]
    public void Evaluate_ContainsSenUnit_ReturnsTrue()
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvision = "Has SEN unit"
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
            ResourcedProvision = provision
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_OnlyResourcedProvision_ReturnsFalse()
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvision = "Has resourced provision"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_BothSenAndResourced_ReturnsTrue()
    {
        // Arrange
        var establishment = new Establishment
        {
            ResourcedProvision = "Has SEN unit and resourced provision"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().BeTrue();
    }
}
