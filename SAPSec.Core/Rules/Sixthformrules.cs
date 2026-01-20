using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has sixth form based on OfficialSixthFormId.
/// Single Responsibility: Only handles sixth form logic.
/// </summary>
public sealed class Sixthformrules : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        return establishment.OfficialSixthFormId switch
        {
            SixthFormCodes.HasSixthForm => DataAvailability.Available(true),
            SixthFormCodes.NoSixthForm => DataAvailability.Available(false),
            SixthFormCodes.NotApplicable => DataAvailability.NotApplicable<bool>(),
            _ => DataAvailability.NotAvailable<bool>()
        };
    }
}