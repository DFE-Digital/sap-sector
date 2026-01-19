using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has sixth form based on OfficialSixthFormId.
/// Single Responsibility: Only handles sixth form logic.
/// </summary>
public sealed class SixthFormRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        return establishment.OfficialSixthFormId switch
        {
            SixthFormCodes.HasSixthForm => DataWithAvailability<bool>.Available(true),
            SixthFormCodes.NoSixthForm => DataWithAvailability<bool>.Available(false),
            SixthFormCodes.NotApplicable => DataWithAvailability<bool>.NotApplicable(),
            _ => DataWithAvailability<bool>.NotAvailable()
        };
    }
}