using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;

/// <summary>
/// Business rule: Determines governance structure based on establishment type and trust membership.
/// Single Responsibility: Only handles governance determination logic.
/// </summary>
public sealed class GovernanceRule : IBusinessRule<GovernanceType>
{
    public DataWithAvailability<GovernanceType> Evaluate(Establishment establishment)
    {
        var typeId = establishment.TypeOfEstablishmentId;
        var typeName = establishment.TypeOfEstablishmentName;
        var hasTrust = !string.IsNullOrWhiteSpace(establishment.TrustsId);

        // Academy/Free school types
        if (IsAcademyType(typeId, typeName))
        {
            return hasTrust
                ? DataWithAvailability.Available(GovernanceType.MultiAcademyTrust)
                : DataWithAvailability.Available(GovernanceType.SingleAcademyTrust);
        }

        // Local Authority maintained
        if (EstablishmentTypeClassification.IsLocalAuthorityMaintained(typeId))
        {
            return DataWithAvailability.Available(GovernanceType.LocalAuthorityMaintained);
        }

        // Non-maintained special school
        if (EstablishmentTypeClassification.IsNonMaintainedSpecialSchool(typeId))
        {
            return DataWithAvailability.Available(GovernanceType.NonMaintainedSpecialSchool);
        }

        // Independent
        if (EstablishmentTypeClassification.IsIndependent(typeId))
        {
            return DataWithAvailability.Available(GovernanceType.Independent);
        }

        // Further/Higher education
        if (EstablishmentTypeClassification.IsFurtherEducation(typeId))
        {
            return DataWithAvailability.Available(GovernanceType.FurtherHigherEducation);
        }

        // Has a type but doesn't match known categories
        if (!string.IsNullOrWhiteSpace(typeId))
        {
            return DataWithAvailability.Available(GovernanceType.Other);
        }

        return DataWithAvailability.NotAvailable<GovernanceType>();
    }

    private static bool IsAcademyType(string? typeId, string? typeName)
    {
        // First check by ID (preferred)
        if (EstablishmentTypeClassification.IsAcademy(typeId))
            return true;

        // Fallback to name-based detection (case-insensitive)
        return EstablishmentTypeClassification.IsAcademyByName(typeName);
    }
}
