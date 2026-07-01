using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Core.Constants;
using SAPSec.Data.Dto;

/// <summary>
/// Business rule: Determines governance structure based on establishment type and trust membership.
/// Single Responsibility: Only handles governance determination logic.
/// </summary>
public sealed class GovernanceRule : IBusinessRule<GovernanceStructure>
{

    //for the frontend
    //return the text required by the FE property
    //including the not available
    public DataWithAvailability<GovernanceStructure> Evaluate(Establishment establishment)
    {
        return GetGovernanceStructure(establishment.TrustSchoolFlagId, establishment.EstablishmentTypeGroupId);

        //would jsut return a string to the FE
        //what about data not available - there is no data not available only no known group
        // return 
        //DataWithAvailability

        //var typeId = establishment.TypeOfEstablishmentId;
        //var typeName = establishment.TypeOfEstablishmentName;
        //var hasTrust = !string.IsNullOrWhiteSpace(establishment.TrustId);

        //// Academy/Free school types
        //if (IsAcademyType(typeId, typeName))
        //{
        //    return hasTrust
        //        ? DataWithAvailability.Available(GovernanceType.MultiAcademyTrust)
        //        : DataWithAvailability.Available(GovernanceType.SingleAcademyTrust);
        //}

        //// Local Authority maintained
        //if (EstablishmentTypeClassification.IsLocalAuthorityMaintained(typeId))
        //{
        //    return DataWithAvailability.Available(GovernanceType.LocalAuthorityMaintained);
        //}

        //// Non-maintained special school
        //if (EstablishmentTypeClassification.IsNonMaintainedSpecialSchool(typeId))
        //{
        //    return DataWithAvailability.Available(GovernanceType.NonMaintainedSpecialSchool);
        //}

        //// Independent
        //if (EstablishmentTypeClassification.IsIndependent(typeId))
        //{
        //    return DataWithAvailability.Available(GovernanceType.Independent);
        //}

        //// Further/Higher education
        //if (EstablishmentTypeClassification.IsFurtherEducation(typeId))
        //{
        //    return DataWithAvailability.Available(GovernanceType.FurtherHigherEducation);
        //}

        //// Has a type but doesn't match known categories
        //if (!string.IsNullOrWhiteSpace(typeId))
        //{
        //    return DataWithAvailability.Available(GovernanceType.Other);
        //}

        //return DataWithAvailability.NotAvailable<GovernanceType>();
    }

    //this just returns the governance structure
    public DataWithAvailability<GovernanceStructure> Evaluate(SimilarSchool similarSchool)
    {
        return GetGovernanceStructure(similarSchool.TrustSchoolFlag.Id, similarSchool.EstablishmentTypeGroup.Id);
    }


    private static DataWithAvailability<GovernanceStructure> GetGovernanceStructure(string trustSchoolFlagId, string establishmentGroupTypeId)
    {
        if (string.IsNullOrEmpty(trustSchoolFlagId) || string.IsNullOrEmpty(establishmentGroupTypeId))
        {
            return DataWithAvailability.NotAvailable<GovernanceStructure>();
        }

        //IsSuportedBySingleAcademyTrust(trustSchoolFlagId)
        if (TrustSchoolFlagValues.IsSupportedBySingleAcademyTrust(trustSchoolFlagId))
        {
            return DataWithAvailability.Available(new GovernanceStructure("S", "Single-academy trust (SAT)"));
        }
        //IsSupportedByMultiAcademyTrust(trustSchoolFlagId)
        if (TrustSchoolFlagValues.IsSupportedByMultiAcademyTrust(trustSchoolFlagId))
        {
            return DataWithAvailability.Available(new GovernanceStructure("M", "Multi-academy trust (SAT)"));
        }

        //IsSupportedByTrust
        //IsNotSupportedByTrust
        //IsNotApplicable
        //IsLaMaintainedSchool
        if ((TrustSchoolFlagValues.IsSupportedByTrust(trustSchoolFlagId) || TrustSchoolFlagValues.IsNotSupportedByTrust(trustSchoolFlagId)) || 
            (TrustSchoolFlagValues.IsNotApplicable(trustSchoolFlagId) && EstablishmentGroupTypeValues.IsLaMaintainedSchool(establishmentGroupTypeId)))
        {
            return DataWithAvailability.Available(new GovernanceStructure("MS", "Maintained school - local authority controlled"));
        }
        //if (trustSchoolFlagId is "1" or "2" || trustSchoolFlagId == "0" && establishmentGroupTypeId == "4")
        //{
        //    return DataWithAvailability.Available(new GovernanceStructure("MS", "Maintained school - local authority controlled"));
        //}

        return DataWithAvailability.Available(new GovernanceStructure("N", "No known group"));
    }


    //private static bool IsAcademyType(string? typeId, string? typeName)
    //{
    //    // First check by ID (preferred)
    //    if (EstablishmentTypeClassification.IsAcademy(typeId))
    //        return true;

    //    // Fallback to name-based detection (case-insensitive)
    //    return EstablishmentTypeClassification.IsAcademyByName(typeName);
    //}
}