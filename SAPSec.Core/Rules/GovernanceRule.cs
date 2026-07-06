using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;


namespace SAPSec.Core.Rules;
/// <summary>
/// Business rule: Determines governance structure based on establishment group type and trust membership.
/// Single Responsibility: Only handles governance determination logic.
/// </summary>
public sealed class GovernanceRule : IBusinessRule<GovernanceType>
{
    public DataWithAvailability<GovernanceType> Evaluate(Establishment establishment)
    {

        var trustSchoolFlagId = establishment.TrustSchoolFlagId;
        var establishmentGroupTypeId = establishment.EstablishmentTypeGroupId;

        if (string.IsNullOrEmpty(trustSchoolFlagId) || string.IsNullOrEmpty(establishmentGroupTypeId))
        {
            return DataWithAvailability.NotAvailable<GovernanceType>();
        }

        if (TrustSchoolFlagValues.IsSupportedBySingleAcademyTrust(trustSchoolFlagId))
        {
            return DataWithAvailability.Available(GovernanceType.SingleAcademyTrust);
        }

        if (TrustSchoolFlagValues.IsSupportedByMultiAcademyTrust(trustSchoolFlagId))
        {
            return DataWithAvailability.Available(GovernanceType.MultiAcademyTrust);
        }

        if ((TrustSchoolFlagValues.IsSupportedByTrust(trustSchoolFlagId) || TrustSchoolFlagValues.IsNotSupportedByTrust(trustSchoolFlagId)) ||
            (TrustSchoolFlagValues.IsNotApplicable(trustSchoolFlagId) && EstablishmentGroupTypeValues.IsLaMaintainedSchool(establishmentGroupTypeId)))
        {
            return DataWithAvailability.Available(GovernanceType.LocalAuthorityMaintained);
        }

        return DataWithAvailability.NotAvailable<GovernanceType>();
    }
}