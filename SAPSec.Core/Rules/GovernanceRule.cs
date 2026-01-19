using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using GovernanceType = SAPSec.Core.Model.GovernanceType;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines governance structure based on establishment type and trust membership.
/// Single Responsibility: Only handles governance determination logic.
/// </summary>
public sealed class GovernanceRule : IBusinessRule<GovernanceType>
{
    public DataWithAvailability<GovernanceType> Evaluate(Establishment establishment)
    {
        var typeId = establishment.TypeOfEstablishmentId;
        var typeName = establishment.TypeOfEstablishmentName?.ToLower() ?? "";
        var hasTrust = !string.IsNullOrWhiteSpace(establishment.TrustsId);

        // Academy/Free school types
        if (IsAcademyType(typeId, typeName))
        {
            return hasTrust
                ? DataWithAvailability<GovernanceType>.Available(GovernanceType.MultiAcademyTrust)
                : DataWithAvailability<GovernanceType>.Available(GovernanceType.SingleAcademyTrust);
        }

        // Local Authority maintained
        if (EstablishmentTypeClassification.IsLocalAuthorityMaintained(typeId))
        {
            return DataWithAvailability<GovernanceType>.Available(GovernanceType.LocalAuthorityMaintained);
        }

        // Non-maintained special school
        if (EstablishmentTypeClassification.IsNonMaintainedSpecialSchool(typeId))
        {
            return DataWithAvailability<GovernanceType>.Available(GovernanceType.NonMaintainedSpecialSchool);
        }

        // Independent
        if (EstablishmentTypeClassification.IsIndependent(typeId))
        {
            return DataWithAvailability<GovernanceType>.Available(GovernanceType.Independent);
        }

        // Further/Higher education
        if (EstablishmentTypeClassification.IsFurtherEducation(typeId))
        {
            return DataWithAvailability<GovernanceType>.Available(GovernanceType.FurtherHigherEducation);
        }

        // Has a type but doesn't match known categories
        if (!string.IsNullOrWhiteSpace(typeId))
        {
            return DataWithAvailability<GovernanceType>.Available(GovernanceType.Other);
        }

        return DataWithAvailability<GovernanceType>.NotAvailable();
    }

    private static bool IsAcademyType(string? typeId, string typeName)
    {
        if (EstablishmentTypeClassification.IsAcademy(typeId))
            return true;

        // Fallback to name-based detection
        return typeName.Contains("academy") ||
               typeName.Contains("free school") ||
               typeName.Contains("studio school") ||
               typeName.Contains("university technical college");
    }
}