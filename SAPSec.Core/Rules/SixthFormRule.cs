using SAPSec.Core.Constants;
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
    public static readonly string HasSixthFormText = "Has a sixth form";
    public static readonly string NoSixthFormText = "Does not have a sixth form";

    public DataWithAvailability<string> Evaluate(Establishment establishment)
    {
        var officialSixthFormId = establishment.OfficialSixthFormId;

        return officialSixthFormId switch
        {
            SixthFormCodes.HasSixthForm => DataWithAvailability.Available(HasSixthFormText),
            SixthFormCodes.NoSixthForm => DataWithAvailability.Available(NoSixthFormText),
            SixthFormCodes.NotApplicable => DataWithAvailability.NotApplicable<string>(),
            _ => DataWithAvailability.NotAvailable<string>()
        };
    }
}