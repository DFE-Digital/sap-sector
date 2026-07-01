using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Core.Constants;
using SAPSec.Data.Dto;


namespace SAPSec.Core.Rules;
/// <summary>
/// Business rule: Determines governance structure based on establishment type and trust membership.
/// Single Responsibility: Only handles governance determination logic.
/// </summary>
public sealed class GovernanceRule : IBusinessRule<GovernanceStructure>
{
    public static readonly string SingleAcademyTrust = "Single-cademy Trust";

    public static readonly string MultiAcademyTrust = "Multi-cademy Trust";

    public static readonly string MaintainedSchool = "Maintained school - local authority controlled";

    public static readonly string NoKnownGroup = "No known group";
    //for the frontend
    //return the text required by the FE property
    //including the not available
    public DataWithAvailability<GovernanceStructure> Evaluate(Establishment establishment)
    {
        return GetGovernanceStructure(establishment.TrustSchoolFlagId, establishment.EstablishmentTypeGroupId);
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