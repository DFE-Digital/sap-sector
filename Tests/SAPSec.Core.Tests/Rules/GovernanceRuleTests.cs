using SAPSec.Core.Model;
using SAPSec.Core.Rules;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Tests.Rules;

public class GovernanceRuleTests
{
    private readonly GovernanceRule _sut = new();

    #region Governance type tests

    [Theory]
    [InlineData("5", "4", GovernanceType.SingleAcademyTrust)]
    [InlineData("3", "4", GovernanceType.MultiAcademyTrust)]
    [InlineData("1", "4", GovernanceType.LocalAuthorityMaintained)]
    [InlineData("2", "4", GovernanceType.LocalAuthorityMaintained)]
    public void Evaluate_Returns_Available_Response(string trustSchoolFlagId, string establishmentTypeGroupId, GovernanceType expectedGovernanceType)
    {
        // Arrange
        var establishment = new Establishment
        {
            EstablishmentTypeGroupId = establishmentTypeGroupId,
            TrustSchoolFlagId = trustSchoolFlagId
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(expectedGovernanceType);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("6", "5")]
    public void Evaluate_Returns_NotAvailable_Response(string trustSchoolFlagId, string establishmentTypeGroupId)
    {
        // Arrange
        var establishment = new Establishment
        {
            EstablishmentTypeGroupId = establishmentTypeGroupId,
            TrustSchoolFlagId = trustSchoolFlagId
        };

        // Act
        var result = _sut.Evaluate(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }
    #endregion
}