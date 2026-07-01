using SAPSec.Core.Constants;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has sixth form based on OfficialSixthFormId.
/// Single Responsibility: Only handles sixth form logic.
/// </summary>
public sealed class SixthFormRule : IBusinessRule<string>
{
    public DataWithAvailability<string> Evaluate(Establishment establishment)
    {
        return GetSixthForm(establishment.OfficialSixthFormId);
    }

    public DataWithAvailability<string> GetSixthForm(string officialSixthFormId)
    {
        return officialSixthFormId switch
        {
            SixthFormCodes.HasSixthForm => DataWithAvailability.Available("Has a sixth form"),
            SixthFormCodes.NoSixthForm => DataWithAvailability.Available("Does not have a sixth form"),
            SixthFormCodes.NotApplicable => DataWithAvailability.NotApplicable<string>(),
            _ => DataWithAvailability.NotAvailable<string>()
        };
    }

    public DataWithAvailability<string> Evaluate(SimilarSchool similarSchool)
    {
        throw new NotImplementedException();
    }
}