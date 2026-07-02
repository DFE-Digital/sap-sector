using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;


namespace SAPSec.Core.Rules;
/// <summary>
/// Business rule: Determines governance structure based on establishment group type and trust membership.
/// Single Responsibility: Only handles governance determination logic.
/// </summary>
public sealed class GovernanceRule : IBusinessRule<string>
{
    public static readonly string SingleAcademyTrust = "Single-academy Trust";
    public static readonly string MultiAcademyTrust = "Multi-academy Trust";
    public static readonly string MaintainedSchool = "Maintained school - local authority controlled";
    public static readonly string NoKnownGroup = "No known group";

    public DataWithAvailability<string> Evaluate(Establishment establishment)
    {

        var trustSchoolFlagId = establishment.TrustSchoolFlagId;
        var establishmentGroupTypeId = establishment.EstablishmentTypeGroupId;

        if (string.IsNullOrEmpty(trustSchoolFlagId) || string.IsNullOrEmpty(establishmentGroupTypeId))
        {
            return DataWithAvailability.NotAvailable<string>();
        }

        if (TrustSchoolFlagValues.IsSupportedBySingleAcademyTrust(trustSchoolFlagId))
        {
            return DataWithAvailability.Available(SingleAcademyTrust);
        }

        if (TrustSchoolFlagValues.IsSupportedByMultiAcademyTrust(trustSchoolFlagId))
        {
            return DataWithAvailability.Available(MultiAcademyTrust);
        }

        if ((TrustSchoolFlagValues.IsSupportedByTrust(trustSchoolFlagId) || TrustSchoolFlagValues.IsNotSupportedByTrust(trustSchoolFlagId)) ||
            (TrustSchoolFlagValues.IsNotApplicable(trustSchoolFlagId) && EstablishmentGroupTypeValues.IsLaMaintainedSchool(establishmentGroupTypeId)))
        {
            return DataWithAvailability.Available(MaintainedSchool);
        }

        return DataWithAvailability.Available(NoKnownGroup);
    }
}