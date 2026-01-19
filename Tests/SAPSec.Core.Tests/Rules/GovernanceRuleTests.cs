using FluentAssertions;
using SAPSec.Core.Model;
using SAPSec.Core.Rules;

namespace SAPSec.Core.Tests.Rules;

public class GovernanceRuleTests
{
    private readonly GovernanceRule _sut = new();

    #region Academy with Trust Tests

    [Theory]
    [InlineData("28")]  // Academy sponsor led
    [InlineData("33")]  // Academy special sponsor led
    [InlineData("34")]  // Academy converter
    [InlineData("35")]  // Free schools
    [InlineData("36")]  // Free schools special
    [InlineData("44")]  // Academy special converter
    [InlineData("45")]  // Academy 16-19 converter
    public void Evaluate_AcademyWithTrust_ReturnsMAT(string typeId)
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = typeId,
            TrustsId = "5001"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(GovernanceType.MultiAcademyTrust);
    }

    #endregion

    #region Academy without Trust Tests

    [Theory]
    [InlineData("28", null)]
    [InlineData("34", "")]
    [InlineData("35", "  ")]
    public void Evaluate_AcademyWithoutTrust_ReturnsSAT(string typeId, string? trustId)
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = typeId,
            TrustsId = trustId
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(GovernanceType.SingleAcademyTrust);
    }

    #endregion

    #region LA Maintained Tests

    [Theory]
    [InlineData("1")]   // Community school
    [InlineData("2")]   // Voluntary aided school
    [InlineData("3")]   // Voluntary controlled school
    [InlineData("5")]   // Foundation school
    [InlineData("7")]   // Community special school
    [InlineData("12")]  // Foundation special school
    [InlineData("14")]  // Pupil referral unit
    [InlineData("15")]  // LA nursery school
    public void Evaluate_LAMaintainedTypes_ReturnsLAMaintained(string typeId)
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = typeId
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(GovernanceType.LocalAuthorityMaintained);
    }

    #endregion

    #region Other School Types Tests

    [Fact]
    public void Evaluate_NonMaintainedSpecial_ReturnsCorrectType()
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = "8"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(GovernanceType.NonMaintainedSpecialSchool);
    }

    [Theory]
    [InlineData("10")]
    [InlineData("11")]
    public void Evaluate_IndependentTypes_ReturnsIndependent(string typeId)
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = typeId
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(GovernanceType.Independent);
    }

    [Theory]
    [InlineData("18")]  // Further education
    [InlineData("29")]  // Higher education institutions
    [InlineData("32")]  // Special post 16 institution
    public void Evaluate_FurtherEducationTypes_ReturnsFurtherEducation(string typeId)
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = typeId
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(GovernanceType.FurtherHigherEducation);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Evaluate_UnknownTypeId_ReturnsOther()
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = "999"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(GovernanceType.Other);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Evaluate_NoTypeId_ReturnsNotAvailable(string? typeId)
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = typeId
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailability.NotAvailable);
    }

    [Theory]
    [InlineData("Academy converter")]
    [InlineData("Free school")]
    [InlineData("Studio school")]
    [InlineData("University technical college")]
    public void Evaluate_AcademyByName_DetectsCorrectly(string typeName)
    {
        // Arrange
        var establishment = new Establishment
        {
            TypeOfEstablishmentId = "999", // Unknown ID
            TypeOfEstablishmentName = typeName,
            TrustsId = "5001"
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(GovernanceType.MultiAcademyTrust);
    }

    #endregion
}