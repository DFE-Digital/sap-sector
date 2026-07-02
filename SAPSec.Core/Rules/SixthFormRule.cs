using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has sixth form based on OfficialSixthFormId.
/// Single Responsibility: Only handles sixth form logic.
/// </summary>
public sealed class SixthFormRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        var officialSixthFormId = establishment.OfficialSixthFormId;

        return officialSixthFormId switch
        {
            SixthFormValues.HasSixthForm => DataWithAvailability.Available(true),
            SixthFormValues.NoSixthForm => DataWithAvailability.Available(false),
            SixthFormValues.NotApplicable => DataWithAvailability.NotApplicable<bool>(),
            _ => DataWithAvailability.NotAvailable<bool>()
        };
    }
}